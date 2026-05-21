# 行测备考资料整理项目

行测（行政职业能力测验）五大模块 + 申论的备考资料整理。

## 源文件清单

根目录下 7 个 `.docx` 文件为原始资料，每个对应一个模块：

| 文件 | 模块 | 目标目录 |
|------|------|----------|
| `xingce_changshi.docx` | 常识判断 | `docs/changshi/` |
| `xingce_yanyu.docx` | 言语理解 | `docs/yanyu/` |
| `xingce_shuliang.docx` | 数量关系 | `docs/shuliang/` |
| `xingce_panduan.docx` | 判断推理 | `docs/panduan/` |
| `tuili.docx` | 推理（图推等） | `docs/panduan/` |
| `ziliao.docx` | 资料分析 | `docs/ziliao/` |
| `os_essays.docx` | 申论（写作） | `docs/shenlun/` |

## 工作流

1. **提取**：将 `.docx` 转为 Markdown（用 `markitdown` 或 `python-docx`）
2. **分类**：按模块放入 `docs/<module>/` 下对应子目录
3. **归档**：原始 `.docx` 移入 `archive/` 以保持根目录整洁

## 目录结构

```
├── docs/          # 转换后的 Markdown 文档
│   ├── changshi/
│   ├── yanyu/
│   ├── shuliang/
│   ├── panduan/
│   ├── ziliao/
│   └── shenlun/
├── notes/         # 学习笔记（手写/整理）
├── scripts/       # 格式转换等辅助脚本
└── archive/       # 已处理的原始 .docx
```

## 约定

- 文件命名：`小写字母_下划线.md`
- 编码：UTF-8
- 内容语言：中文
- 每篇文档推荐在开头加 H1 标题和简要说明
