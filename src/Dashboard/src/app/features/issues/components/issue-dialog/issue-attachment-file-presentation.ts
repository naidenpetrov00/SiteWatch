export function getIssueAttachmentFileIcon(
  contentType: string,
  fileName: string
): string {
  if (contentType === 'application/pdf') return 'picture_as_pdf';
  if (contentType.includes('zip') || /\.(zip|rar|7z|tar|gz)$/i.test(fileName)) return 'folder_zip';
  if (contentType.includes('spreadsheet') || /\.(xls|xlsx|csv)$/i.test(fileName)) return 'table_view';
  if (contentType.includes('word') || /\.(doc|docx|odt|txt)$/i.test(fileName)) return 'description';
  if (contentType.startsWith('audio/')) return 'audio_file';
  return 'draft';
}
