import { Icon, Label, Stack, TextField } from "@fluentui/react";
import React from "react";
import { buttonClassName, iconClassName } from "../../Pages/Home/homePage.styles";
import { FileVersionsService } from "../../services";
import { errorMessageClassName, modalContainerClassName, nameStyles } from "./editFileVersionNameModal.styles";
import { EditFileVersionNameModalProps } from "./editFileVersionNameModal.types";

export const EditFileVersionNameModal = (props: EditFileVersionNameModalProps): JSX.Element => {
    const [name, setName] = React.useState<string>("");
    const [errorMessage, setErrorMessage] = React.useState<string>("");

    React.useEffect(() => {
        if (errorMessage !== "") {
            setErrorMessage("");
        }
    }, [name]);

    const handleModifyName = (): void => {
        let newErrorMessage: string = "";
        if (name === "") {
            newErrorMessage += "Name can't be empty.";
        }

        FileVersionsService.UpdateFileVersionName(props.fileVersion.id as string, { jwt: localStorage.getItem("jwt") as string, newFileName: name })
            .then(function (response) {
                props.onModifiedName(props.fileVersion, name)
            })
            .catch(function (error) {
                props.onError(error);
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
            <button className={buttonClassName} onClick={handleModifyName}>
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