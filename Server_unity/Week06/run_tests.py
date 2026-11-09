# run_tests.py
# pytestをインストールできない環境向けの簡易テストランナー。
# test_app.py をそのまま実行し、フィクスチャ（client / tmp_path / monkeypatch）を
# 引数名で注入する。pytestが使える環境では、これではなく `pytest -v` を使うこと。
import inspect
import pathlib
import sys
import tempfile
import traceback
import types

# test_app.py の `import pytest` と `@pytest.fixture` を満たすスタブ
_stub = types.ModuleType("pytest")
_stub.fixture = lambda f: f
sys.modules["pytest"] = _stub

import test_app  # noqa: E402


class MonkeyPatch:
	def __init__(self):
		self._undo = []

	def setattr(self, obj, name, value):
		self._undo.append((obj, name, getattr(obj, name)))
		setattr(obj, name, value)

	def undo(self):
		for obj, name, value in reversed(self._undo):
			setattr(obj, name, value)


def run_one(name):
	with tempfile.TemporaryDirectory() as tmp:
		available = {"tmp_path": pathlib.Path(tmp), "monkeypatch": MonkeyPatch()}
		gen = test_app.client(available["tmp_path"], available["monkeypatch"])
		available["client"] = next(gen)  # yield までを実行
		try:
			func = getattr(test_app, name)
			args = [available[p] for p in inspect.signature(func).parameters]
			func(*args)
		finally:
			try:
				next(gen)  # フィクスチャの後処理
			except StopIteration:
				pass
			available["monkeypatch"].undo()


def main():
	passed = failed = 0
	for name in sorted(n for n in dir(test_app) if n.startswith("test_")):
		try:
			run_one(name)
			print(f"PASSED  {name}")
			passed += 1
		except Exception:
			print(f"FAILED  {name}")
			traceback.print_exc()
			failed += 1
	print(f"\n{passed} passed, {failed} failed")
	sys.exit(1 if failed else 0)


if __name__ == "__main__":
	main()
