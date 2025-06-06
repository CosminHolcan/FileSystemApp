import { Checkbox, Dropdown, IDropdownOption, Icon, Label, Stack, TextField } from "@fluentui/react";
import React from "react";
import { ICreateFileDTO as IAddFileDTO } from "../../DTO/CreateFileDTO";
import { FileLocation } from "../../Enums/FileLocation";
import { Redundancy } from "../../Enums/Redundancy";
import { buttonClassName, iconClassName } from "../../Pages/Home/homePage.styles";
import { AppFilesService, FileVersionsService } from "../../services";
import { IsNullOrUndefined } from "../../utils";
import { errorMessageClassName, modalContainerClassName, nameStyles, versionNameStyles } from "./addVersionModal.styles";
import { AddVersionModalProps } from "./addVersionModal.types";
import { IFileVersion } from "../../Models/FileVersion";
import { IAddFileVersionDTO } from "../../DTO/AddFileVersionDTO";

export const AddVersionModal = (props: AddVersionModalProps): JSX.Element => {
    const [name, setName] = React.useState<string>("");
    const [file, setFile] = React.useState<File | null>(null);
    const [errorMessage, setErrorMessage] = React.useState<string>("");

    React.useEffect(() => {
        if (errorMessage !== "") {
            setErrorMessage("");
        }
    }, [name, file]);

    const onFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files && e.target.files[0];
        if (file) {
            setFile(file);
        }
    };

    const handleAddVersion = (): void => {
        let newErrorMessage: string = "";
        if (name === "") {
            newErrorMessage += "Name can't be empty.";
        }

        if (IsNullOrUndefined(file)) {
            newErrorMessage += " Select a file.";
            return;
        }

        if (file?.name.split('.').pop() !== props.originalFileName.split('.').pop()) {
            newErrorMessage += " Select a file of the same type.";
        }

        if (newErrorMessage !== "") {
            setErrorMessage(newErrorMessage);
            return;
        }

        const newVersionFile: IAddFileVersionDTO = {
            jwt: localStorage.getItem("jwt") as string,
            id: "00000000-0000-0000-0000-000000000000",
            name: name,
            originalFileId: props.originalFileId
        };

        const formData = new FormData();
        formData.append('dto', JSON.stringify(newVersionFile))
        formData.append('file', file as File);

        FileVersionsService.AddVersion(formData)
            .then(function (response) {
                props.onAddedVersion(response.data)
            })
            .catch(function (error) {
                console.log(error);
            });
    };

    return (
        <Stack className={modalContainerClassName} verticalAlign="space-between">
            <Stack horizontal>
                <TextField
                    value={name}
                    onChange={(event, newValue) => setName(newValue ?? "")}
                    styles={nameStyles}
                    label="Version Name"
                />
            </Stack>
            <Stack>
                <Label>File</Label>
                <input
                    type="file"
                    onChange={onFileChange}
                />
            </Stack>
            <button className={buttonClassName} onClick={handleAddVersion}>
                <Icon
                    className={iconClassName}
                    iconName="Save"
                />
                Save
            </button>
            {errorMessage !== "" &&
                <Label className={errorMessageClassName}>
                    {errorMessage}
                </Label>
            }
        </Stack>
    )
}