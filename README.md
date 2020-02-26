# Auto Image Compresser

## How to use

1. Create configure section to `configure.ini`
	* default `configure.ini` has `vrchat` configure (for VRChat camera)
1. Launch `AutoImageCompresser(CreateShortcut).exe`.
	* When launched, this application makes per-ini-section shortcuts.
1. You can compress from a per-ini-section shortcut.


## INI section

| key                          | required `default` | value                               | description                                                                                                      |
|:-----------------------------|:-------------------|:------------------------------------|:-----------------------------------------------------------------------------------------------------------------|
| `input_dir`                  | **yes**            | Directory                           | Input/Observe directory.                                                                                         |
| `output_dir`                 | **yes**            | Directory                           | Output directory.                                                                                                |
| `type`                       | **yes**            | File Type (`png`, `jpg`)            | Output File Type                                                                                                 |
| `quality`                    | **yes if jpg**     | Integer (0-100)                     | Output JPG quality                                                                                               |
| `mode`                       | **yes**            | Mode (`observe`, `oneshot`, `both`) | observe (Observing input directory and compress.), oneshot (Find and compress uncompressed files.), both (Both.) |
| `pattern`                    | no `*`             | Glob Pattern                        | Input file glob pattern                                                                                          |
| `create_output_dir`          | no `false`         | Boolean (`true`, `false`)           | If set to `true`, Create `output_dir` if it doesn't exist.                                                       |
| `observe_file_read_delay_ms` | no `1000`          | Integer (0-)                        | Delay from file-system notification to compress. (Waiting for file write)                                        |
| `max_long_side` †1           | no `0`             | Integer (0, 1-)                     | Long side size limitation (0: unconfigured)                                                                      |
| `max_width` †1               | no `0`             | Integer (0, 1-)                     | Width limitation          (0: unconfigured)                                                                      |
| `max_height` †1              | no `0`             | Integer (0, 1-)                     | Height limitation         (0: unconfigured)                                                                      |

### †1
If all values are unconfigured, this application when not change resolution.

## Build

### Deps
* Windows 10
* Cygwin
	* git
	* make
	* zip

```bash
git clone https://github.com/yanorei32/AutoImageCompresser
cd AutoImageCompresser
make genzip # zip create
make # minimal
make clean # clean
```

