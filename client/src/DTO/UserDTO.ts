import { IBaseDTO } from "./BaseDTO";
import { IRegisterUserDTO } from "./RegisterUserDTO";

export interface IUserDto extends IBaseDTO, IRegisterUserDTO {
    id: string;
};