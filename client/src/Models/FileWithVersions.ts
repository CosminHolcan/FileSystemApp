import { IFileVersion } from "./FileVersion";

export interface IFileWithVersions {
    id: string;
    name: string;
    fileVersions: IFileVersion[]
};