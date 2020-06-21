from pathlib import Path
from shutil import copy

TARGET = Path(r'D:/programs/unturned_server/steamapps/common/Unturned/Servers/dev/Rocket/Plugins').absolute()
FILE = Path(r'bin/debug/CustomSpawnpoints.dll').absolute()

if not TARGET.exists():
    TARGET.mkdir(parents=True)

copy(FILE, TARGET)
