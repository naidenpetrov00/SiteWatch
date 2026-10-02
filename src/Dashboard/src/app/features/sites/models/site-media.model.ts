export interface SiteImageMedia {
  imageId: string;
  thumbnailId: string;
  category: string;
  created: string;
}

export interface SiteVideoMedia {
  videoId: string;
  snapshotId: string;
  durationSeconds: number | null;
  category: string;
  created: string;
}

export interface SiteFileMedia {
  fileId: string;
  fileName: string;
  contentType: string;
  documentType: string;
  created: string;
}

export interface SiteMediaCollection {
  images: readonly SiteImageMedia[];
  videos: readonly SiteVideoMedia[];
  files: readonly SiteFileMedia[];
}

export type SiteMediaKind = 'images' | 'videos' | 'files';

export const FILE_DOCUMENT_TYPES = [
  ['Warranty', 'Warranty'],
  ['DrawingOrSchema', 'Drawing or schema'],
  ['ProductDatasheetOrTechnicalSpecification', 'Product datasheet or technical specification'],
  ['InstallationOrOperationManual', 'Installation or operation manual'],
  ['CertificateOrComplianceDocument', 'Certificate or compliance document'],
  ['InspectionOrServiceReport', 'Inspection or service report'],
  ['PermitOrApproval', 'Permit or approval'],
  ['Other', 'Other']
] as const;

export function formatDocumentType(value: string): string {
  return FILE_DOCUMENT_TYPES.find(([key]) => key === value)?.[1] ?? value;
}
