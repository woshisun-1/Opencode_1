#!/usr/bin/env python3
"""
本地图片处理工具
支持：文字查找/替换/删除、Logo/二维码移除、敏感内容打码
所有处理在本地完成，无网络请求
"""

import argparse
import json
import os
import sys
import shutil
import tempfile
from pathlib import Path


def check_dependencies():
    """检查并提示缺失依赖"""
    missing = []
    try:
        import PIL
    except ImportError:
        missing.append("pillow")
    try:
        import cv2
    except ImportError:
        missing.append("opencv-python-headless")
    try:
        import numpy
    except ImportError:
        missing.append("numpy")
    
    if missing:
        print(json.dumps({
            "code": "DEP_MISSING",
            "message": f"缺少依赖: {', '.join(missing)}",
            "fix": f"pip install {' '.join(missing)}"
        }))
        sys.exit(1)


def get_ocr_engine():
    """获取可用的 OCR 引擎，优先 easyocr（中文好），其次 pytesseract"""
    try:
        import easyocr
        return ("easyocr", easyocr)
    except ImportError:
        pass
    
    try:
        import pytesseract
        from PIL import Image
        pytesseract.get_tesseract_version()
        return ("tesseract", pytesseract)
    except (ImportError, Exception):
        pass
    
    return (None, None)


def ocr_image(cv_img, engine_hint=None):
    """对图片进行 OCR，返回统一格式的结果列表
    
    返回: [(text, confidence, bbox_4pts, rect_xyxy)]
    """
    import cv2
    import numpy as np
    
    name, engine = get_ocr_engine()
    
    if name == "easyocr":
        reader = engine.Reader(['ch_sim', 'en'], gpu=False)
        raw = reader.readtext(cv_img)
        results = []
        for bbox, text, confidence in raw:
            pts = np.array(bbox, dtype=np.int32)
            x_min = int(min(p[0] for p in pts))
            y_min = int(min(p[1] for p in pts))
            x_max = int(max(p[0] for p in pts))
            y_max = int(max(p[1] for p in pts))
            results.append((text, float(confidence), pts, (x_min, y_min, x_max, y_max)))
        return results
    
    if name == "tesseract":
        from PIL import Image
        pil_rgb = Image.fromarray(cv2.cvtColor(cv_img, cv2.COLOR_BGR2RGB))
        data = engine.image_to_data(pil_rgb, lang='chi_sim+eng', output_type=engine.Output.DICT)
        results = []
        n = len(data['text'])
        for i in range(n):
            text = (data['text'][i] or '').strip()
            if not text:
                continue
            conf = int(data['conf'][i]) / 100.0 if data['conf'][i] != '-1' else 0.0
            x, y, w, h = data['left'][i], data['top'][i], data['width'][i], data['height'][i]
            if w < 2 or h < 2:
                continue
            x1, y1, x2, y2 = x, y, x + w, y + h
            pts = np.array([[x1, y1], [x2, y1], [x2, y2], [x1, y2]], dtype=np.int32)
            results.append((text, conf, pts, (x1, y1, x2, y2)))
        return results
    
    return None


def load_image(path):
    """加载图片，返回 PIL Image 和 OpenCV 格式"""
    import cv2
    import numpy as np
    from PIL import Image
    
    if not os.path.exists(path):
        print(json.dumps({"code": "FILE_ERR", "message": f"文件不存在: {path}"}))
        sys.exit(1)
    
    ext = Path(path).suffix.lower()
    if ext not in ('.png', '.jpg', '.jpeg', '.bmp', '.tiff', '.webp'):
        print(json.dumps({"code": "FMT_ERR", "message": f"不支持的格式: {ext}，仅支持 PNG/JPG/BMP/TIFF/WEBP"}))
        sys.exit(1)
    
    size_mb = os.path.getsize(path) / (1024 * 1024)
    if size_mb > 50:
        print(json.dumps({"code": "SIZE_ERR", "message": f"文件过大 ({size_mb:.1f}MB)，请压缩至 50MB 以内"}))
        sys.exit(1)
    
    pil_img = Image.open(path).convert("RGB")
    cv_img = np.array(pil_img)
    cv_img = cv2.cvtColor(cv_img, cv2.COLOR_RGB2BGR)
    return pil_img, cv_img


def save_output(cv_img, output_dir=None):
    """保存处理后的图片"""
    import cv2
    import numpy as np
    from PIL import Image
    
    if output_dir:
        out_dir = Path(output_dir)
    else:
        out_dir = Path(tempfile.gettempdir()) / "image_processor"
    out_dir.mkdir(parents=True, exist_ok=True)
    
    import time
    import random
    ts = int(time.time() * 1000)
    rid = f"{random.randint(0, 0xFFFFFFFF):08x}"
    out_path = out_dir / f"{ts}_{rid}.png"
    
    cv_rgb = cv2.cvtColor(cv_img, cv2.COLOR_BGR2RGB)
    Image.fromarray(cv_rgb).save(str(out_path), "PNG")
    
    return str(out_path)


def cmd_ocr(args):
    """OCR 识别，返回文本位置信息"""
    check_dependencies()
    import cv2
    import numpy as np
    
    pil_img, cv_img = load_image(args.path)
    results = ocr_image(cv_img)
    
    if results is None:
        print(json.dumps({
            "code": "DEP_MISSING",
            "message": "需要 OCR 引擎。选项: 1) pip install easyocr（推荐，中文好，需下载约2GB） 2) 安装 Tesseract-OCR 后 pip install pytesseract",
            "fix": "pip install easyocr"
        }))
        sys.exit(1)
    
    items = []
    for text, confidence, pts, rect in results:
        x1, y1, x2, y2 = rect
        items.append({
            "text": text,
            "confidence": round(confidence, 4),
            "bbox": [[int(p[0]), int(p[1])] for p in pts],
            "rect": [x1, y1, x2, y2]
        })
    
    print(json.dumps({
        "code": 0,
        "data": {
            "items": items,
            "total": len(items)
        }
    }, ensure_ascii=False))


def cmd_find(args):
    """在图片中查找指定文字"""
    check_dependencies()
    import cv2
    import numpy as np
    
    pil_img, cv_img = load_image(args.path)
    results = ocr_image(cv_img)
    
    if results is None:
        print(json.dumps({
            "code": "DEP_MISSING",
            "message": "需要 OCR 引擎。选项: 1) pip install easyocr（推荐，中文好，需下载约2GB） 2) 安装 Tesseract-OCR 后 pip install pytesseract",
            "fix": "pip install easyocr"
        }))
        sys.exit(1)
    
    target = args.text.lower()
    matches = []
    for text, confidence, pts, rect in results:
        if target in text.lower():
            x1, y1, x2, y2 = rect
            matches.append({
                "text": text,
                "confidence": round(confidence, 4),
                "rect": [x1, y1, x2, y2]
            })
    
    print(json.dumps({
        "code": 0,
        "data": {
            "found": len(matches) > 0,
            "matches": matches,
            "total": len(matches)
        }
    }, ensure_ascii=False))


def inpaint_region(cv_img, rect, method=None):
    """对指定矩形区域进行修复（去除内容）"""
    import cv2
    import numpy as np
    if method is None:
        method = cv2.INPAINT_TELEA
    
    x, y, w, h = rect
    h_img, w_img = cv_img.shape[:2]
    x = max(0, x); y = max(0, y)
    w = min(w, w_img - x); h = min(h, h_img - y)
    
    mask = np.zeros((h_img, w_img), dtype=np.uint8)
    pad = max(2, int(min(w, h) * 0.05))
    x1 = max(0, x - pad); y1 = max(0, y - pad)
    x2 = min(w_img, x + w + pad); y2 = min(h_img, y + h + pad)
    mask[y1:y2, x1:x2] = 255
    
    result = cv2.inpaint(cv_img, mask, 3, method)
    return result


def cmd_redact(args):
    """删除/涂抹图片中的指定文字"""
    check_dependencies()
    import cv2
    import numpy as np
    
    pil_img, cv_img = load_image(args.path)
    mode = args.mode or "inpaint"
    results = ocr_image(cv_img)
    
    if results is None:
        print(json.dumps({
            "code": "DEP_MISSING",
            "message": "需要 OCR 引擎。选项: 1) pip install easyocr（推荐，中文好，需下载约2GB） 2) 安装 Tesseract-OCR 后 pip install pytesseract",
            "fix": "pip install easyocr"
        }))
        sys.exit(1)
    
    target = args.text.lower()
    found_any = False
    
    for text, confidence, pts, rect in results:
        if target in text.lower():
            found_any = True
            x1, y1, x2, y2 = rect
            w = x2 - x1
            h = y2 - y1
            
            pad = max(2, int(min(w, h) * 0.1))
            bx1 = max(0, x1 - pad)
            by1 = max(0, y1 - pad)
            bx2 = min(cv_img.shape[1], x2 + pad)
            by2 = min(cv_img.shape[0], y2 + pad)
            
            if mode == "blur":
                roi = cv_img[by1:by2, bx1:bx2]
                ksize = max(15, max(w, h) // 2)
                if ksize % 2 == 0:
                    ksize += 1
                blurred = cv2.GaussianBlur(roi, (ksize, ksize), 30)
                cv_img[by1:by2, bx1:bx2] = blurred
            elif mode == "solid":
                cv_img[by1:by2, bx1:bx2] = (255, 255, 255)
            elif mode == "inpaint":
                cv_img = inpaint_region(cv_img, (bx1, by1, bx2-bx1, by2-by1))
    
    out_path = save_output(cv_img, args.output)
    print(json.dumps({
        "code": 0,
        "data": {
            "path": out_path,
            "found": found_any,
            "mode": mode
        }
    }, ensure_ascii=False))


def cmd_replace(args):
    """查找并替换图片中的文字"""
    check_dependencies()
    import cv2
    import numpy as np
    from PIL import Image, ImageDraw, ImageFont
    
    pil_img, cv_img = load_image(args.path)
    results = ocr_image(cv_img)
    
    if results is None:
        print(json.dumps({
            "code": "DEP_MISSING",
            "message": "需要 OCR 引擎。选项: 1) pip install easyocr（推荐，中文好，需下载约2GB） 2) 安装 Tesseract-OCR 后 pip install pytesseract",
            "fix": "pip install easyocr"
        }))
        sys.exit(1)
    
    target = args.find.lower()
    replacement = args.replace
    match_case = args.match_case
    found_any = False
    
    for text, confidence, pts, rect in results:
        text_to_check = text if match_case else text.lower()
        target_to_check = target if match_case else target.lower()
        
        if target_to_check in text_to_check:
            found_any = True
            x1, y1, x2, y2 = rect
            w = x2 - x1
            h = y2 - y1
            
            pad = max(2, int(min(w, h) * 0.1))
            bx1 = max(0, x1 - pad)
            by1 = max(0, y1 - pad)
            bx2 = min(cv_img.shape[1], x2 + pad)
            by2 = min(cv_img.shape[0], y2 + pad)
            
            cv_img = inpaint_region(cv_img, (bx1, by1, bx2-bx1, by2-by1))
            
            pil_rgb = Image.fromarray(cv2.cvtColor(cv_img, cv2.COLOR_BGR2RGB))
            draw = ImageDraw.Draw(pil_rgb)
            
            font_size = max(12, h)
            font = None
            font_paths = [
                "C:/Windows/Fonts/msyh.ttc",
                "C:/Windows/Fonts/simhei.ttf",
                "C:/Windows/Fonts/arial.ttf",
                "/usr/share/fonts/truetype/noto/NotoSansCJK-Regular.ttc",
                "/System/Library/Fonts/PingFang.ttc",
            ]
            for fp in font_paths:
                if os.path.exists(fp):
                    try:
                        font = ImageFont.truetype(fp, font_size)
                        break
                    except:
                        continue
            
            replace_text = replacement
            bbox_w = bx2 - bx1
            bbox_h = by2 - by1
            
            if font:
                try:
                    bbox_text = draw.textbbox((0, 0), replace_text, font=font)
                    tw = bbox_text[2] - bbox_text[0]
                    th = bbox_text[3] - bbox_text[1]
                    if tw > bbox_w:
                        scale = bbox_w / tw
                        new_size = max(8, int(font_size * scale))
                        try:
                            font = ImageFont.truetype(font.path, new_size)
                        except:
                            pass
                except:
                    pass
                
                try:
                    bbox_text = draw.textbbox((0, 0), replace_text, font=font)
                    tw = bbox_text[2] - bbox_text[0]
                    th = bbox_text[3] - bbox_text[1]
                except:
                    tw, th = bbox_w, bbox_h
            else:
                char_w = font_size * 0.6
                tw = len(replace_text) * char_w
                th = font_size
            
            tx = bx1 + (bbox_w - tw) // 2
            ty = by1 + (bbox_h - th) // 2 + int(th * 0.85)
            
            if font:
                draw.text((tx, ty), replace_text, fill=(0, 0, 0), font=font)
            else:
                draw.text((tx, ty), replace_text, fill=(0, 0, 0))
            
            cv_img = cv2.cvtColor(np.array(pil_rgb), cv2.COLOR_RGB2BGR)
    
    out_path = save_output(cv_img, args.output)
    print(json.dumps({
        "code": 0,
        "data": {
            "path": out_path,
            "found": found_any,
            "find": args.find,
            "replace": replacement
        }
    }, ensure_ascii=False))


def cmd_remove_region(args):
    """删除图片中指定区域的内容"""
    check_dependencies()
    import cv2
    import numpy as np
    
    pil_img, cv_img = load_image(args.path)
    
    regions = []
    if args.regions:
        for r in args.regions.split(";"):
            parts = r.strip().split(",")
            if len(parts) == 4:
                x, y, w, h = map(int, parts)
                regions.append((x, y, w, h))
    
    if not regions and args.xywh:
        parts = args.xywh.split(",")
        if len(parts) == 4:
            x, y, w, h = map(int, parts)
            regions.append((x, y, w, h))
    
    if not regions:
        print(json.dumps({
            "code": "PARAM_ERR",
            "message": "请指定区域, 格式: --xywh x,y,w,h 或 --regions 'x1,y1,w1,h1;x2,y2,w2,h2'"
        }))
        sys.exit(1)
    
    for x, y, w, h in regions:
        if args.mode == "blur":
            roi = cv_img[y:y+h, x:x+w]
            ksize = max(15, max(w, h) // 4)
            if ksize % 2 == 0:
                ksize += 1
            roi = cv2.GaussianBlur(roi, (ksize, ksize), 30)
            cv_img[y:y+h, x:x+w] = roi
        elif args.mode == "solid":
            cv_img[y:y+h, x:x+w] = (255, 255, 255)
        elif args.mode == "inpaint":
            cv_img = inpaint_region(cv_img, (x, y, w, h))
    
    out_path = save_output(cv_img, args.output)
    print(json.dumps({
        "code": 0,
        "data": {
            "path": out_path,
            "regions_processed": len(regions),
            "mode": args.mode
        }
    }))


def cmd_remove_qrcode(args):
    """检测并移除图片中的二维码/条形码"""
    check_dependencies()
    import cv2
    import numpy as np
    
    pil_img, cv_img = load_image(args.path)
    
    qr_detector = cv2.QRCodeDetector()
    data, pts, _ = qr_detector.detectAndDecode(cv_img)
    
    found_any = False
    
    if pts is not None:
        found_any = True
        pts = np.array(pts, dtype=np.int32)
        x_min = int(min(p[0] for p in pts[0]))
        y_min = int(min(p[1] for p in pts[0]))
        x_max = int(max(p[0] for p in pts[0]))
        y_max = int(max(p[1] for p in pts[0]))
        pad = 10
        x1 = max(0, x_min - pad); y1 = max(0, y_min - pad)
        x2 = min(cv_img.shape[1], x_max + pad)
        y2 = min(cv_img.shape[0], y_max + pad)
        cv_img = inpaint_region(cv_img, (x1, y1, x2-x1, y2-y1))
    
    try:
        from pyzbar.pyzbar import decode as pyzbar_decode
        barcodes = pyzbar_decode(cv_img)
        if barcodes:
            found_any = True
            for barcode in barcodes:
                poly = barcode.polygon
                xs = [p.x for p in poly]
                ys = [p.y for p in poly]
                x1 = max(0, min(xs) - 5); y1 = max(0, min(ys) - 5)
                x2 = min(cv_img.shape[1], max(xs) + 5)
                y2 = min(cv_img.shape[0], max(ys) + 5)
                cv_img = inpaint_region(cv_img, (x1, y1, x2-x1, y2-y1))
    except ImportError:
        pass
    
    out_path = save_output(cv_img, args.output)
    print(json.dumps({
        "code": 0,
        "data": {
            "path": out_path,
            "found": found_any,
            "qr_data": data if data else None
        }
    }, ensure_ascii=False))


def cmd_remove_logo(args):
    """通过模板匹配或手动区域删除Logo"""
    check_dependencies()
    import cv2
    import numpy as np
    
    pil_img, cv_img = load_image(args.path)
    
    found_any = False
    
    if args.logo_path:
        logo_pil = Image.open(args.logo_path).convert("RGB")
        logo_cv = cv2.cvtColor(np.array(logo_pil), cv2.COLOR_RGB2BGR)
        lh, lw = logo_cv.shape[:2]
        
        result = cv2.matchTemplate(cv_img, logo_cv, cv2.TM_CCOEFF_NORMED)
        threshold = args.threshold or 0.7
        locations = np.where(result >= threshold)
        
        for pt in zip(*locations[::-1]):
            found_any = True
            x, y = pt[0], pt[1]
            pad = 5
            x1 = max(0, x - pad); y1 = max(0, y - pad)
            x2 = min(cv_img.shape[1], x + lw + pad)
            y2 = min(cv_img.shape[0], y + lh + pad)
            cv_img = inpaint_region(cv_img, (x1, y1, x2-x1, y2-y1))
    
    if args.xywh:
        parts = args.xywh.split(",")
        if len(parts) == 4:
            found_any = True
            x, y, w, h = map(int, parts)
            if args.mode == "inpaint":
                cv_img = inpaint_region(cv_img, (x, y, w, h))
            elif args.mode == "blur":
                roi = cv_img[y:y+h, x:x+w]
                ksize = max(25, max(w, h) // 3)
                if ksize % 2 == 0:
                    ksize += 1
                cv_img[y:y+h, x:x+w] = cv2.GaussianBlur(roi, (ksize, ksize), 30)
            else:
                cv_img[y:y+h, x:x+w] = (255, 255, 255)
    
    if not found_any:
        print(json.dumps({
            "code": 0,
            "data": {
                "path": None,
                "found": False,
                "message": "未找到匹配的Logo，请手动指定区域 --xywh x,y,w,h"
            }
        }))
        return
    
    out_path = save_output(cv_img, args.output)
    print(json.dumps({
        "code": 0,
        "data": {
            "path": out_path,
            "found": True
        }
    }))


def cmd_batch(args):
    """批量处理目录中的所有图片"""
    input_dir = Path(args.input_dir)
    if not input_dir.exists():
        print(json.dumps({"code": "DIR_ERR", "message": f"目录不存在: {args.input_dir}"}))
        sys.exit(1)
    
    image_exts = ('.png', '.jpg', '.jpeg', '.bmp', '.tiff', '.webp')
    files = [f for f in input_dir.iterdir() if f.suffix.lower() in image_exts]
    
    if not files:
        print(json.dumps({"code": "NO_FILE", "message": f"目录中无图片文件: {args.input_dir}"}))
        sys.exit(1)
    
    results = []
    for f in files:
        result = {"file": str(f), "status": "pending"}
        try:
            args.path = str(f)
            if args.action == "redact":
                cmd_redact(args)
            elif args.action == "replace":
                cmd_replace(args)
            elif args.action == "remove-qrcode":
                cmd_remove_qrcode(args)
            result["status"] = "ok"
        except Exception as e:
            result["status"] = "error"
            result["error"] = str(e)
        results.append(result)
    
    print(json.dumps({
        "code": 0,
        "data": {
            "results": results,
            "total": len(results),
            "success": sum(1 for r in results if r["status"] == "ok"),
            "failed": sum(1 for r in results if r["status"] == "error")
        }
    }, ensure_ascii=False))


def main():
    parser = argparse.ArgumentParser(description="本地图片处理工具")
    parser.add_argument("--output", "-o", help="输出目录")
    
    subparsers = parser.add_subparsers(dest="command", title="子命令")
    
    # ocr
    p_ocr = subparsers.add_parser("ocr", help="OCR 识别图片文字")
    p_ocr.add_argument("--path", "-p", required=True, help="图片路径")
    
    # find
    p_find = subparsers.add_parser("find", help="查找图片中指定文字")
    p_find.add_argument("--path", "-p", required=True, help="图片路径")
    p_find.add_argument("--text", "-t", required=True, help="要查找的文字")
    
    # redact
    p_redact = subparsers.add_parser("redact", help="删除/涂抹指定文字")
    p_redact.add_argument("--path", "-p", required=True, help="图片路径")
    p_redact.add_argument("--text", "-t", required=True, help="要删除的文字")
    p_redact.add_argument("--mode", "-m", choices=["blur", "solid", "inpaint"], default="inpaint",
                         help="删除模式: blur=模糊, solid=纯色块, inpaint=修复(默认)")
    
    # replace
    p_repl = subparsers.add_parser("replace", help="查找并替换文字")
    p_repl.add_argument("--path", "-p", required=True, help="图片路径")
    p_repl.add_argument("--find", required=True, help="要查找的文字")
    p_repl.add_argument("--replace", required=True, help="替换为")
    p_repl.add_argument("--match-case", action="store_true", help="区分大小写")
    
    # remove-region
    p_rr = subparsers.add_parser("remove-region", help="删除指定区域内容")
    p_rr.add_argument("--path", "-p", required=True, help="图片路径")
    p_rr.add_argument("--xywh", help="区域坐标 x,y,w,h")
    p_rr.add_argument("--regions", help="多个区域 x1,y1,w1,h1;x2,y2,w2,h2")
    p_rr.add_argument("--mode", choices=["blur", "solid", "inpaint"], default="inpaint")
    
    # remove-qrcode
    p_qr = subparsers.add_parser("remove-qrcode", help="检测并移除二维码/条形码")
    p_qr.add_argument("--path", "-p", required=True, help="图片路径")
    
    # remove-logo
    p_logo = subparsers.add_parser("remove-logo", help="移除Logo")
    p_logo.add_argument("--path", "-p", required=True, help="图片路径")
    p_logo.add_argument("--logo-path", help="Logo模板图片路径（用于自动匹配）")
    p_logo.add_argument("--xywh", help="手动指定Logo区域 x,y,w,h")
    p_logo.add_argument("--threshold", type=float, default=0.7, help="模板匹配阈值 (0-1, 默认0.7)")
    p_logo.add_argument("--mode", choices=["blur", "solid", "inpaint"], default="inpaint")
    
    # batch
    p_batch = subparsers.add_parser("batch", help="批量处理目录中所有图片")
    p_batch.add_argument("--input-dir", "-i", required=True, help="输入目录")
    p_batch.add_argument("--action", required=True, choices=["redact", "replace", "remove-qrcode"],
                        help="批量操作类型")
    p_batch.add_argument("--text", help="(redact/replace 用) 要查找的文字")
    p_batch.add_argument("--replace", help="(replace 用) 替换为")
    p_batch.add_argument("--mode", default="inpaint", help="处理模式")
    
    args = parser.parse_args()
    
    if not args.command:
        parser.print_help()
        sys.exit(1)
    
    if args.command == "ocr":
        cmd_ocr(args)
    elif args.command == "find":
        cmd_find(args)
    elif args.command == "redact":
        cmd_redact(args)
    elif args.command == "replace":
        cmd_replace(args)
    elif args.command == "remove-region":
        cmd_remove_region(args)
    elif args.command == "remove-qrcode":
        cmd_remove_qrcode(args)
    elif args.command == "remove-logo":
        cmd_remove_logo(args)
    elif args.command == "batch":
        cmd_batch(args)


if __name__ == "__main__":
    main()
