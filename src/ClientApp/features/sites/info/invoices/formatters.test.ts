import {
  formatInvoiceAmount,
  formatInvoiceDate,
  formatInvoiceDateTime,
  formatInvoiceText,
} from "./formatters";

describe("invoice formatters", () => {
  it("renders missing values as a consistent placeholder", () => {
    expect(formatInvoiceAmount(null)).toBe("—");
    expect(formatInvoiceDate(null)).toBe("—");
    expect(formatInvoiceDateTime(null)).toBe("—");
    expect(formatInvoiceText("   ")).toBe("—");
  });

  it("keeps invalid dates visible rather than replacing them", () => {
    expect(formatInvoiceDate("not-a-date")).toBe("not-a-date");
    expect(formatInvoiceDateTime("not-a-date")).toBe("not-a-date");
  });

  it("formats invoice amounts with two fraction digits", () => {
    expect(formatInvoiceAmount(12)).toMatch(/12[.,]00/);
  });
});
