// stdafx.h : 標準のシステム インクルード ファイルのインクルード ファイル、または
// 参照回数が多く、かつあまり変更されない、プロジェクト専用のインクルード ファイル
// を記述します。
//

#pragma once

#include "targetver.h"

#define WIN32_LEAN_AND_MEAN             // Windows ヘッダーから使用されていない部分を除外します。
// Windows ヘッダー ファイル:
#include <windows.h>
#include <Dbt.h>
#include <setupapi.h>
#include <stdio.h>
#include <string.h>
#include <iostream>

#pragma comment( lib, "setupapi.lib" )

// TODO: プログラムに必要な追加ヘッダーをここで参照してください。
//#define DEVICE_ID_NUM	16
