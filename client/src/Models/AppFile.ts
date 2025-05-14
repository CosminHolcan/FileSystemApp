import { FileLocation } from "../Enums/FileLocation";
import { Redundancy } from "../Enums/Redundancy";

export interface IAppFile {
    id?: string;
    name?: string;
    storageAccountId?: string;
    location?: FileLocation;
    secondaryLocation?: FileLocation;
    redundancy?: Redundancy;
    versioning?: boolean;
    versionName?: string;
    tokenSAS?: string;
};