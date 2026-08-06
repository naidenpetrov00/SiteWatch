import { Text } from "react-native";
import { fireEvent, render, screen } from "@testing-library/react-native";
import type React from "react";

import InvoiceDetailsModal from "./InvoiceDetailsModal";

jest.mock("@/hooks/useColorPalette", () => ({
  useColorPalette: () => ({ background: "white", contrastText: "white", primary: "blue", secondary: "gray", text: "black" }),
}));
jest.mock("react-native-safe-area-context", () => ({
  SafeAreaView: ({ children }: { children: React.ReactNode }) => <>{children}</>,
}));
jest.mock("@expo/vector-icons/Ionicons", () => () => <Text>Icon</Text>);

describe("InvoiceDetailsModal", () => {
  const invoice = {
    id: "invoice-1",
    numberId: 42,
    isComplete: false,
    supplierId: null,
    supplierDisplayLabel: null,
    submittedFromSiteName: "North site",
    invoiceNumber: null,
    date: null,
    created: "2026-01-01T00:00:00Z",
    taxIdentifier: null,
    address: null,
    email: null,
    phoneNumber: null,
    contactPerson: null,
    paymentTerm: null,
    totalValueExcludingVat: null,
    vatRate: null,
    vat: null,
    totalValueIncludingVat: null,
    paymentDate: null,
    paymentTime: null,
    paymentMethod: null,
    siteAllocation: null,
  };

  it("does not render until an invoice is selected", () => {
    render(<InvoiceDetailsModal invoice={null} onClose={jest.fn()} />);

    expect(screen.queryByText("Invoice details")).toBeNull();
  });

  it("shows incomplete invoice placeholders and closes through its accessible action", () => {
    const onClose = jest.fn();
    render(<InvoiceDetailsModal invoice={invoice} onClose={onClose} />);

    expect(screen.getByText("Invoice #42")).toBeOnTheScreen();
    expect(screen.getByText("North site")).toBeOnTheScreen();
    expect(screen.getAllByText("—").length).toBeGreaterThan(0);

    fireEvent.press(screen.getByRole("button", { name: "Close invoice details" }));
    expect(onClose).toHaveBeenCalledTimes(1);
  });
});
