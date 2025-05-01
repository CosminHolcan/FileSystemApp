import { IAppFile } from "../Models/AppFile";
import { IBaseDTO } from "./BaseDTO";

export interface ICreateFileDTO extends IBaseDTO, IAppFile {
};