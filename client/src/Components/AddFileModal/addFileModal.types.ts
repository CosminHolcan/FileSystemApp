import { IAppFile } from "../../Models/AppFile";

export interface AddFileModalProps {
    onAddedFile: (newFile: IAppFile) => void;
    onError: (error: any) => void;
};
