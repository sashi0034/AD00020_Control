AD00020: https://github.com/bit-trade-one/AD00020-USB_IR_Remote_Controller

x86 向けにビルドする (x_86-64 (64-bit) は不可)

![screenshot.png](screenshot.png)

デフォルトの `settings/settings.json{

```json
{
  "power_on": {
    "bytes": [
      "0140980220E004000000060220E00400213480AF000006604000800006B60000000000",
      "0140980220E004000000060220E00400253480AF0068410B4000800006080000000000",
      "0140980220E004000000060220E00400213480AF000006604000800006B60000000000",
      "0140980220E004000000060220E00400253480AF0068410B4000800006080000000000"
    ],
    "comment": "26℃除湿3時間設定 (2回実行)"
  },
  "power_off": {
    "bytes": [
      "0140980220E004000000060220E00400203480AF000006604000800006B50000000000",
      "0140980220E004000000060220E00400203480AF000006604000800006B50000000000"
    ],
    "comment": "スイッチ OFF (2回実行)"
  },
  "job": [
    {
      "hour": 12,
      "command": "power_on"
    },
    {
      "hour": 13,
      "command": "power_off"
    },
    {
      "hour": 15,
      "command": "power_on"
    },
    {
      "hour": 16,
      "command": "power_off"
    },
    {
      "hour": 18,
      "command": "power_on"
    },
    {
      "hour": 19,
      "command": "power_off"
    }
  ]
```
