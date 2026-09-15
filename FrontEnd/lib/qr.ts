import QRCode from 'qrcode';

/**
 * Renders a QR as an inline SVG string on the server.
 *
 * Inline SVG keeps the code in the initial HTML, so a visitor's camera can read
 * it before any JS hydrates and it stays crisp when the sheet is printed for an
 * exhibition stand.
 */
export async function renderQrSvg(
  value: string,
  { margin = 1 }: { margin?: number } = {},
): Promise<string | null> {
  if (!value) return null;

  try {
    const svg = await QRCode.toString(value, {
      type: 'svg',
      margin,
      // Survives a printed sheet being scuffed or partially covered.
      errorCorrectionLevel: 'M',
      color: { dark: '#14221a', light: '#ffffff' },
    });

    // The library emits a fixed width/height; let CSS size it instead.
    return svg.replace(/<svg([^>]*)>/, (match, attrs: string) => {
      const cleaned = attrs
        .replace(/\swidth="[^"]*"/, '')
        .replace(/\sheight="[^"]*"/, '');
      return `<svg${cleaned} width="100%" height="100%" role="img">`;
    });
  } catch {
    return null;
  }
}
