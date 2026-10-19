# Revit Family DLL Builder

A public agent skill for creating parametric Autodesk Revit families with C#
generator DLLs, loaded through **Add-in Manager → Manual Mode**.

It turns manufacturer PDFs, submittals, CAD, or reference RFAs into source-based
generators: catalogue types, native geometry, materials, real MEP connectors when
applicable, Desktop logs, runtime flex checks, and save/reopen validation.

## Install in Codex

Ask Codex's skill installer:

```text
Use $skill-installer to install the skill from
https://github.com/moustafamagdi/revit-family-dll-skill/tree/main/skills/revit-family-dll
```

Or copy `skills/revit-family-dll` into your agent's supported skills directory.
Current Codex documentation lists `~/.agents/skills` for user skills and
`.agents/skills` for repository skills; older hosts may differ. See the
[official skill documentation](https://learn.chatgpt.com/docs/build-skills).
If a personal skill has the same name, choose deliberately which version to use.

## Use

Attach source documents and ask:

```text
Use $revit-family-dll to build a Revit 2024 generator DLL from this manufacturer
PDF. Include documented catalogue sizes, real piping connectors, useful
installation parameters, source notes, and a Desktop log. I will run it through
Add-in Manager myself. Tell me which checks passed and which still need Revit.
```

The skill delivers DLLs and editable C# source, rather than a finished manufacturer
family library. It uses the requested Revit version; the demonstration starter
specifically targets **Revit 2024 / net48 / x64**.

## Included

| Path inside `skills/revit-family-dll` | Purpose |
| --- | --- |
| `SKILL.md` | Agent workflow |
| `references/` | Build, geometry, connector, and validation guidance |
| `scripts/build.ps1` | Compile a copied Revit 2024 starter/project |
| `assets/starter/` | Native box, height association, three synthetic types, logs, runtime assertions |
| `agents/openai.yaml` | Codex display metadata |

The starter has **no MEP connectors** and no manufacturer dimensions. Product
generators must implement their own geometry and connectors. Copy before editing.

## Requirements and testing

Compilation needs Windows, a .NET SDK, the .NET Framework 4.8 targeting pack, and
local Revit 2024 API assemblies. Running the demo needs Revit 2024, compatible
Add-in Manager, and a user-selected Generic Model RFT. Autodesk binaries,
templates, and Add-in Manager are not redistributed.

Adapt later-release projects against their actual SDK. Revit 2025 uses .NET 8;
this net48 starter is not a universal build. See
[Autodesk's migration guidance](https://help.autodesk.com/view/RVT/2025/ENU/?caas=caas/blog/thebuildingcoder.typepad.com/blog/2024/04/migrating-from-net-48-to-net-core-8.html).

See [VALIDATION.md](VALIDATION.md) for exact release verification. Compilation
does not prove a correct Revit run. Assertions in source are not executed tests.

No account, API key, Revit connector, or remote service is required by the skill.
An agent needs local file/build tools. Direct Revit execution requires user
authorization and a suitable local environment. Logs can contain local paths;
review them before public sharing.

## العربية

مهارة عامة لإنشاء عائلات Revit عن طريق DLL بلغة C# وتشغيله من Add-in Manager
في وضع Manual Mode. تشمل الأبعاد الموثقة والأنواع والبارامترات الفعلية ووصلات
MEP عند الحاجة وسجل تشخيص على سطح المكتب. يوضح التسليم الفرق بين نجاح التجميع
ونجاح الاختبار داخل Revit. المثال المرفق مخصص لـ Revit 2024؛ يمكن تكييف منهج
العمل لإصدارات أخرى باستخدام SDK مناسب.

## License

[MIT](LICENSE) for original instructions/code. Manufacturer documents and
Autodesk products retain their own licenses.

## Privacy, terms, and support

Published by Mostafa Magdy. Read the [privacy policy](PRIVACY.md) and
[terms of use](TERMS.md). For help, use the
[public issue tracker](https://github.com/moustafamagdi/revit-family-dll-skill/issues).
Please remove private documents, credentials, and identifying paths before posting.
