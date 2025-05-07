import { FileLocation } from "../Enums/FileLocation";
import { Redundancy } from "../Enums/Redundancy";

export interface IAppFile {
    id?: string;
    name?: string;
    storageAccountId?: string;
    location?: FileLocation;
    redundancy?: Redundancy;
    versioning?: boolean;
    versionName?: string;
};