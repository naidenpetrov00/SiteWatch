import { ALL_FILTER } from "../media-types";

export const FILE_DOCUMENT_TYPES = [
  "Warranty",
  "DrawingOrSchema",
  "ProductDatasheetOrTechnicalSpecification",
  "InstallationOrOperationManual",
  "CertificateOrComplianceDocument",
  "InspectionOrServiceReport",
  "PermitOrApproval",
  "Other",
] as const;

export type FileDocumentType = (typeof FILE_DOCUMENT_TYPES)[number];
export type FileDocumentTypeFilter = typeof ALL_FILTER | FileDocumentType;

export const FILE_DOCUMENT_TYPE_LABELS: Record<FileDocumentType, string> = {
  Warranty: "Warranty",
  DrawingOrSchema: "Drawing / Schema",
  ProductDatasheetOrTechnicalSpecification:
    "Product datasheet / Technical specification",
  InstallationOrOperationManual: "Installation or operation manual",
  CertificateOrComplianceDocument: "Certificate / Compliance document",
  InspectionOrServiceReport: "Inspection / Service report",
  PermitOrApproval: "Permit / Approval",
  Other: "Other",
};

export type SiteFileIds = {
  fileId: string;
  fileName: string;
  contentType: string;
  documentType: FileDocumentType;
  created: string;
};
