const withOpacity = (color: string, opacity: number) => {
  const hex = color.trim().replace(/^#/, "");
  const expandedHex = hex.length === 3 || hex.length === 4
    ? hex.split("").map((character) => `${character}${character}`).join("")
    : hex;
  const rgb = expandedHex.match(/^([0-9a-f]{2})([0-9a-f]{2})([0-9a-f]{2})(?:[0-9a-f]{2})?$/i);

  if (!rgb) return color;

  return `rgba(${parseInt(rgb[1], 16)}, ${parseInt(rgb[2], 16)}, ${parseInt(rgb[3], 16)}, ${opacity})`;
};

export default withOpacity;
