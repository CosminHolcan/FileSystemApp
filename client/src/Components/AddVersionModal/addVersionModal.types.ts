import { IAppFile as IFileVersion } from "../../Models/AppFile";

export interface AddVersionModalProps {
    onAddedVersion: (bewVersion: IFileVersion) => void;
    onErrorAddVersion: (error: any) => void;
    originalFileName: string;
    originalFileId: string;
};
