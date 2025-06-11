import { Icon, Label, Stack, TextField } from "@fluentui/react";
import React from "react";
import { IAddFileVersionDTO } from "../../DTO/AddFileVersionDTO";
import { IBaseDTO } from "../../DTO/BaseDTO";
import { buttonClassName, iconClassName } from "../../Pages/Home/homePage.styles";
import { AppFilesService, FileVersionsService } from "../../services";
import { IsNullOrUndefined } from "../../utils";
import { errorMessageClassName, modalContainerClassName, nameStyles } from "./uploadNewContentModal.styles";
import { UploadNewContentModalProps } from "./uploadNewContentModal.types";

export const UploadNewContentModal = (props: UploadNewContentModalProps): JSX.Element => {
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

        if (IsNullOrUndefined(file)) {
            newErrorMessage += "Select a file.";
            return;
        }

        if (file?.name.split('.').pop() !== props.fileName.split('.').pop()) {
            newErrorMessage += " Select a file of the same type.";
        }

        if (props.versioning && name === "") {
            newErrorMessage += " Name can't be empty.";
        }

        if (newErrorMessage !== "") {
            setErrorMessage(newErrorMessage);
            return;
        }

        if (props.versioning) {
            const newVersionFile: IAddFileVersionDTO = {
                jwt: localStorage.getItem("jwt") as string,
                id: "00000000-0000-0000-0000-000000000000",
                name: name,
                originalFileId: props.fileId
            };

            const formData = new FormData();
            formData.append('dto', JSON.stringify(newVersionFile))
            formData.append('file', file as File);

            FileVersionsService.AddVersion(formData)
                .then(function (response) {
                    props.onAddedContent(response.data)
                })
                .catch(function (error) {
                    props.onErrorAddedContent(error);
                });

            return;
        }

        const dto: IBaseDTO = {
            jwt: localStorage.getItem("jwt") as string
        };

        const formData = new FormData();
        formData.append('dto', JSON.stringify(dto))
        formData.append('file', file as File);

        AppFilesService.UploadNewContent(props.fileId, formData)
            .then(function (response) {
                props.onAddedContent(response.data)
            })
            .catch(function (error) {
                props.onErrorAddedContent(error);
            });
    };

    return (
        <Stack className={modalContainerClassName} verticalAlign="space-between">
            <Stack horizontal>
                {props.versioning &&
                    <TextField
                        value={name}
                        onChange={(event, newValue) => setName(newValue ?? "")}
                        styles={nameStyles}
                        placeholder="Version name"
                    />
                }
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