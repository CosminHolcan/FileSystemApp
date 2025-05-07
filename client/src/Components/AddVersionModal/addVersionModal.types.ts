import { IAppFile as IFileVersion } from "../../Models/AppFile";

export interface AddVersionModalProps {
    onAddedVersion: (bewVersion: IFileVersion) => void;
    originalFileName: string;
    originalFileId: string;
};
