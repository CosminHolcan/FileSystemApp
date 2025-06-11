
export interface UploadNewContentModalProps {
    onAddedContent: (file: any) => void;
    onErrorAddedContent: (error: any) => void;
    fileName: string;
    fileId: string;
    versioning: boolean;
};
