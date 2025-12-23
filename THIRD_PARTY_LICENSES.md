# Third-Party Licenses

Veriflow 3.0 uses the following third-party libraries and components. We are grateful to the open-source community for their contributions.

---

## Core Framework

### .NET 8
- **License**: MIT License
- **Copyright**: © Microsoft Corporation
- **Source**: https://github.com/dotnet/runtime
- **License URL**: https://github.com/dotnet/runtime/blob/main/LICENSE.TXT

---

## UI Framework

### Avalonia UI
- **Version**: 11.2.2
- **License**: MIT License
- **Copyright**: © The Avalonia Project
- **Source**: https://github.com/AvaloniaUI/Avalonia
- **License URL**: https://github.com/AvaloniaUI/Avalonia/blob/master/licence.md

---

## Audio Libraries

### NAudio
- **Version**: 2.2.1
- **License**: MIT License
- **Copyright**: © Mark Heath & Contributors
- **Source**: https://github.com/naudio/NAudio
- **License URL**: https://github.com/naudio/NAudio/blob/master/license.txt

---

## Video Libraries

### LibVLCSharp
- **Version**: 3.9.5
- **License**: LGPL v2.1
- **Copyright**: © VideoLAN
- **Source**: https://code.videolan.org/videolan/LibVLCSharp
- **License URL**: https://www.gnu.org/licenses/old-licenses/lgpl-2.1.html

**LGPL Compliance Notice**:
- LibVLCSharp is dynamically linked via NuGet package
- Users can replace the LibVLC library files
- No modifications have been made to LibVLCSharp source code
- Source code available at: https://code.videolan.org/videolan/LibVLCSharp

### VideoLAN.LibVLC.Windows
- **Version**: 3.0.21
- **License**: LGPL v2.1
- **Copyright**: © VideoLAN
- **Source**: https://www.videolan.org/vlc/
- **License URL**: https://www.gnu.org/licenses/old-licenses/lgpl-2.1.html

**LGPL Compliance Notice**:
- LibVLC is dynamically linked
- Users can replace LibVLC DLL files in the installation directory
- No modifications have been made to LibVLC
- Source code available at: https://code.videolan.org/videolan/vlc

---

## Media Processing

### FFmpeg
- **License**: LGPL v2.1+ (or GPL v2+ depending on build)
- **Copyright**: © FFmpeg developers
- **Source**: https://ffmpeg.org/
- **License URL**: https://www.gnu.org/licenses/old-licenses/lgpl-2.1.html

**LGPL Compliance Notice**:
- FFmpeg is used as an external executable (ffmpeg.exe, ffprobe.exe)
- Users can replace FFmpeg binaries
- No modifications have been made to FFmpeg
- Veriflow uses the LGPL build (no GPL-only codecs)
- Source code available at: https://github.com/FFmpeg/FFmpeg

**Installation**: FFmpeg must be installed separately and available in system PATH.

### FFmpeg.AutoGen
- **Version**: 7.1.0
- **License**: MIT License (wrapper), LGPL (FFmpeg)
- **Copyright**: © Ruslan Balanukhin
- **Source**: https://github.com/Ruslan-B/FFmpeg.AutoGen
- **License URL**: https://github.com/Ruslan-B/FFmpeg.AutoGen/blob/master/LICENSE

---

## PDF Generation

### QuestPDF
- **Version**: 2025.12.0
- **License**: Community License (Non-commercial) / Professional License (Commercial)
- **Copyright**: © QuestPDF
- **Source**: https://github.com/QuestPDF/QuestPDF
- **License URL**: https://github.com/QuestPDF/QuestPDF/blob/main/LICENSE.txt

**⚠️ IMPORTANT NOTICE**:
- QuestPDF Community License is for **non-commercial use only**
- For commercial distribution, a Professional License is required
- See: https://www.questpdf.com/pricing.html
- **Alternative**: Consider replacing with PdfSharp (MIT) for commercial use

---

## MVVM Toolkit

### CommunityToolkit.Mvvm
- **Version**: 8.3.2
- **License**: MIT License
- **Copyright**: © .NET Foundation and Contributors
- **Source**: https://github.com/CommunityToolkit/dotnet
- **License URL**: https://github.com/CommunityToolkit/dotnet/blob/main/License.md

---

## Utilities

### System.IO.Hashing
- **Version**: 9.0.0
- **License**: MIT License
- **Copyright**: © Microsoft Corporation
- **Source**: https://github.com/dotnet/runtime

### MathNet.Numerics
- **Version**: 5.0.0
- **License**: MIT License
- **Copyright**: © Math.NET Project
- **Source**: https://github.com/mathnet/mathnet-numerics
- **License URL**: https://github.com/mathnet/mathnet-numerics/blob/master/LICENSE.md

---

## Testing Libraries (Development Only)

### xUnit
- **Version**: 2.9.2
- **License**: Apache License 2.0
- **Copyright**: © .NET Foundation and Contributors
- **Source**: https://github.com/xunit/xunit

### FluentAssertions
- **Version**: 8.8.0
- **License**: Apache License 2.0
- **Copyright**: © Dennis Doomen and Contributors
- **Source**: https://github.com/fluentassertions/fluentassertions

### Moq
- **Version**: 4.20.72
- **License**: BSD 3-Clause License
- **Copyright**: © Daniel Cazzulino and Contributors
- **Source**: https://github.com/moq/moq4

---

## LGPL Compliance Statement

Veriflow 3.0 uses several libraries licensed under the GNU Lesser General Public License (LGPL):
- LibVLCSharp / LibVLC
- FFmpeg

**Compliance Measures**:

1. **Dynamic Linking**: All LGPL libraries are dynamically linked, not statically compiled
2. **No Modifications**: No modifications have been made to any LGPL library source code
3. **Replaceable**: Users can replace LGPL library files with their own versions
4. **Source Access**: Source code for all LGPL libraries is publicly available
5. **Attribution**: This file provides proper attribution and license information

**How to Replace LGPL Libraries**:

**LibVLC**:
1. Download LibVLC from https://www.videolan.org/vlc/
2. Replace DLL files in: `[Installation Directory]/runtimes/win-x64/native/`

**FFmpeg**:
1. Download FFmpeg from https://ffmpeg.org/download.html
2. Replace `ffmpeg.exe` and `ffprobe.exe` in system PATH or application directory

---

## License Summary

| Component | License | Commercial Use | Modifications |
|-----------|---------|----------------|---------------|
| .NET 8 | MIT | ✅ Allowed | ✅ Allowed |
| Avalonia UI | MIT | ✅ Allowed | ✅ Allowed |
| NAudio | MIT | ✅ Allowed | ✅ Allowed |
| LibVLCSharp | LGPL v2.1 | ✅ Allowed* | ❌ Not Modified |
| FFmpeg | LGPL v2.1 | ✅ Allowed* | ❌ Not Modified |
| QuestPDF | Community/Pro | ⚠️ License Required | N/A |
| CommunityToolkit | MIT | ✅ Allowed | ✅ Allowed |
| MathNet.Numerics | MIT | ✅ Allowed | ✅ Allowed |

*LGPL allows commercial use with dynamic linking and user-replaceable libraries

---

## Acknowledgments

We thank all the developers and contributors of the above projects for their excellent work and for making their software available to the community.

---

## Questions?

For license compliance questions, please contact:
- Email: legal@veriflow.com
- GitHub: https://github.com/Herve-obe/Veriflow_v3.0/issues

---

**Last Updated**: 2025-12-23  
**Veriflow Version**: 3.0.0-beta
