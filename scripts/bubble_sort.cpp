#include <iostream>
#include <utility>
#include <vector>

void bubble_sort(std::vector<int>& arr) {
    if (arr.size() < 2) return;
    for (size_t i = 0; i < arr.size() - 1; ++i) {
        bool swapped = false;
        for (size_t j = 0; j < arr.size() - i - 1; ++j) {
            if (arr[j] > arr[j + 1]) {
                std::swap(arr[j], arr[j + 1]);
                swapped = true;
            }
        }
        if (!swapped) break;
    }
}

int main() {
    std::vector<int> arr = {64, 34, 25, 12, 22, 11, 90};

    std::cout << "排序前: ";
    for (int v : arr) std::cout << v << " ";
    std::cout << "\n";

    bubble_sort(arr);

    std::cout << "排序后: ";
    for (int v : arr) std::cout << v << " ";
    std::cout << "\n";

    return 0;
}
