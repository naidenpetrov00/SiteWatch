import { render, screen, userEvent } from "@testing-library/react-native";

import InvoiceCard from "./InvoiceCard";

jest.mock("@/hooks/useColorPalette", () => ({
  useColorPalette: () => ({
    background: "white",
    contrastText: "white",
    primary: "blue",
    secondary: "gray",
    text: "black",
  }),
}));

describe("InvoiceCard", () => {
  const props = {
    id: "invoice-1",
    numberId: 42,
    invoiceNumber: "INV-42",
    supplierDisplayLabel: "Acme Ltd",
    submittedFromSiteName: "North site",
    date: "2026-01-02T00:00:00Z",
    created: "2026-01-01T00:00:00Z",
    isComplete: true,
    totalValueIncludingVat: 120,
    allocatedAmount: 60,
    isFileActionDisabled: false,
    isOpeningFile: false,
    onSelect: jest.fn(),
    onOpenFile: jest.fn(),
  };

  afterEach(() => jest.clearAllMocks());

  it("shows invoice context and invokes the selected user action", async () => {
    const user = userEvent.setup();
    render(<InvoiceCard {...props} />);

    expect(screen.getByText("INV-42")).toBeOnTheScreen();
    expect(screen.getByText("Acme Ltd")).toBeOnTheScreen();
    expect(screen.getByText("Uploaded from North site")).toBeOnTheScreen();

    await user.press(screen.getByRole("button", { name: "View INV-42" }));
    await user.press(screen.getByRole("button", { name: "Open file for INV-42" }));

    expect(props.onSelect).toHaveBeenCalledWith("invoice-1");
    expect(props.onOpenFile).toHaveBeenCalledWith("invoice-1");
  });

  it("disables file access while another invoice file is opening", () => {
    render(<InvoiceCard {...props} isFileActionDisabled isOpeningFile />);

    expect(screen.getByRole("button", { name: "Open file for INV-42" })).toBeDisabled();
  });
});
