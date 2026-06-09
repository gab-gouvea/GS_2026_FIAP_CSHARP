"""
Gera capturas (PNG) com visual de terminal a partir dos .txt em docs/evidencias/.

Cores reais do terminal (Console.ForegroundColor em ConsoleUtils.cs):
  EscreverSucesso -> Green   (linhas "OK: ...")
  EscreverAviso   -> Yellow  (linhas "AVISO: ..." - cobre "AVISO: [MONITOR] ...")
  EscreverErro    -> Red     (linhas "ERRO: ..."  - cobre "ERRO: [MONITOR] ...")
Nenhuma outra coisa (titulos, subtitulos, banner, CRITICO) tem cor no terminal real,
entao nao colorimos para que o PNG bata 1:1 com a execucao interativa.

Saida: PNG so com o corpo do terminal (sem barra de janela / titulo falso).
"""

from PIL import Image, ImageDraw, ImageFont
from pathlib import Path
import os
import re

BASE = Path(__file__).resolve().parent
EVID = BASE / "evidencias"
OUT = EVID

BG = (28, 28, 30)           # macOS Terminal dark
FG = (220, 220, 220)
PADDING_X = 28
PADDING_Y = 18
LINE_SPACING = 4

FONT_SIZE = 16
LINE_HEIGHT = FONT_SIZE + LINE_SPACING

COLOR_RULES = []  # sem coloração: tudo branco no fundo escuro


def encontrar_fonte():
    for caminho in [
        "/System/Library/Fonts/Menlo.ttc",
        "/Library/Fonts/Menlo.ttc",
        "/System/Library/Fonts/Courier.dfont",
        "/Library/Fonts/Courier New.ttf",
    ]:
        if os.path.exists(caminho):
            return ImageFont.truetype(caminho, FONT_SIZE)
    return ImageFont.load_default()


FONT = encontrar_fonte()

LARGURA_MAX_CARACTERES = 110


def medir_largura(texto: str) -> int:
    bbox = FONT.getbbox(texto or " ")
    return bbox[2] - bbox[0]


def quebrar_linha_longa(linha: str, max_chars: int) -> list[str]:
    if len(linha) <= max_chars:
        return [linha]
    indent = len(linha) - len(linha.lstrip(" "))
    prefixo = " " * (indent + 2)
    palavras = linha.split(" ")
    linhas = []
    atual = ""
    for palavra in palavras:
        candidato = palavra if not atual else f"{atual} {palavra}"
        if len(candidato) > max_chars and atual:
            linhas.append(atual)
            atual = prefixo + palavra
        else:
            atual = candidato
    if atual:
        linhas.append(atual)
    return linhas


def cor_da_linha(linha: str):
    for regex, cor in COLOR_RULES:
        if regex.search(linha):
            return cor
    return FG


def renderizar(linhas, caminho_saida: Path):
    largura_max = max((medir_largura(l) for l in linhas), default=200)
    largura = max(900, largura_max + PADDING_X * 2)
    altura = PADDING_Y * 2 + len(linhas) * LINE_HEIGHT

    img = Image.new("RGB", (largura, altura), BG)
    draw = ImageDraw.Draw(img)

    y = PADDING_Y
    for linha in linhas:
        draw.text((PADDING_X, y), linha, fill=cor_da_linha(linha), font=FONT)
        y += LINE_HEIGHT

    img.save(caminho_saida, "PNG", optimize=True)
    return caminho_saida


def quebrar_em_paginas(linhas, max_linhas: int):
    paginas = []
    inicio = 0
    while inicio < len(linhas):
        fim = min(inicio + max_linhas, len(linhas))
        if fim < len(linhas):
            ajuste = fim
            while ajuste > inicio + int(max_linhas * 0.5) and linhas[ajuste - 1].strip() != "":
                ajuste -= 1
            if ajuste > inicio:
                fim = ajuste
        paginas.append(linhas[inicio:fim])
        inicio = fim
    return paginas


def normalizar(texto: str) -> list[str]:
    linhas = []
    for raw in texto.splitlines():
        linha = raw.replace("\t", "    ").rstrip()
        for fragmento in quebrar_linha_longa(linha, LARGURA_MAX_CARACTERES):
            linhas.append(fragmento)
    while linhas and linhas[0].strip() == "":
        linhas.pop(0)
    while linhas and linhas[-1].strip() == "":
        linhas.pop()
    return linhas


def processar(arquivo: Path, max_linhas_por_pagina: int):
    linhas = normalizar(arquivo.read_text(encoding="utf-8"))
    paginas = quebrar_em_paginas(linhas, max_linhas_por_pagina)
    nome_base = arquivo.stem
    saidas = []
    for idx, pagina in enumerate(paginas, start=1):
        sufixo = "" if len(paginas) == 1 else f"-pag{idx}"
        caminho = OUT / f"{nome_base}{sufixo}.png"
        renderizar(pagina, caminho)
        saidas.append(caminho)
    return saidas


def main():
    EVID.mkdir(parents=True, exist_ok=True)
    pacotes = [
        ("01-cenario-completo.txt", 45),
        ("02-listagens.txt", 50),
        ("03-erro-codigo-duplicado.txt", 60),
    ]
    for nome, max_linhas in pacotes:
        arquivo = EVID / nome
        if not arquivo.exists():
            print(f"[pulando] {arquivo} nao existe.")
            continue
        for saida in processar(arquivo, max_linhas):
            print(f"[ok] {saida.relative_to(BASE.parent)}")


if __name__ == "__main__":
    main()
