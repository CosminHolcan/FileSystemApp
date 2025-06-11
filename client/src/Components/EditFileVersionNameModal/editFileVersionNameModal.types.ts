import { IFileVersion } from "../../Models/FileVersion";

export interface EditFileVersionNameModalProps {
    fileVersion: IFileVersion;
    onModifiedName: (fileVersion: IFileVersion, newName: string) => void;
    onError: (error: any) => void;
};
