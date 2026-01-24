#!/bin/bash
# Generování PDF z deployment dokumentace
# Použití: ./generate-pdf.sh

set -e

echo "🔧 Generuji PDF z deployment dokumentace..."
echo ""

# Nastavení PATH pro TeX
export PATH="/Library/TeX/texbin:$PATH"

# Přejdi do složky projektu
cd /Users/petrsramek/AntigravityProjects/MIMM-2.0

# Seznam souborů k převodu
FILES=(
  "DEPLOYMENT_PLAN_LITE.md"
  "DEPLOYMENT_CHECKLIST_LITE.md"
  "DEPLOYMENT_QUICK_REFERENCE_LITE.md"
  "DEPLOYMENT_CHECKLIST_LITE_DETAILED.md"
)

# Převod každého souboru
for f in "${FILES[@]}"; do
  if [ -f "$f" ]; then
    echo "📄 Generuji: ${f%.md}.pdf"
    pandoc "$f" \
      --pdf-engine=xelatex \
      -V geometry:margin=2cm \
      -V papersize:a4 \
      -V fontsize=11pt \
      -o "${f%.md}.pdf"
    echo "   ✅ Hotovo"
  else
    echo "   ⚠️  Soubor $f nenalezen, přeskakuji"
  fi
done

echo ""
echo "🎉 Všechny PDF vygenerovány!"
echo ""
echo "📋 Seznam PDF souborů:"
ls -lh DEPLOYMENT_*_LITE*.pdf 2>/dev/null || echo "Žádné PDF soubory nenalezeny"

echo ""
echo "✨ Hotovo! PDF najdeš ve stejné složce jako Markdown soubory."
