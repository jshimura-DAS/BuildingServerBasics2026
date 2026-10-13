# main.py
from utils import greet

# utils.py の __name__ は "utils" となり、
# if __name__ == "__main__": ブロックは実行されない
print(greet("Hanako"))
# 出力: こんにちは、Hanako