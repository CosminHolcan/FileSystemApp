import { Checkbox, Dropdown, IDropdownOption, Icon, Label, Stack, TextField } from "@fluentui/react";
import React from "react";
import { ICreateFileDTO } from "../../DTO/CreateFileDTO";
import { FileLocation } from "../../Enums/FileLocation";
import { Redundancy } from "../../Enums/Redundancy";
import { buttonClassName, iconClassName } from "../../Pages/Home/homePage.styles";
import { AppFilesService } from "../../services";
import { IsNullOrUndefined } from "../../utils";
import { errorMessageClassName, modalContainerClassName, titleInputStyles } from "./addFileModal.styles";
import { AddFileModalProps } from "./addFileModal.types";

export const AddFileModal = (props: AddFileModalProps): JSX.Element => {
    const [name, setName] = React.useState<string>("");
    const [file, setFile] = React.useState<File | null>(null);
    const [location, setLocation] = React.useState<FileLocation | null>(null);
    const [redundancy, setRedundancy] = React.useState<Redundancy | null>(null);
    const [versioning, setVersioning] = React.useState<boolean>(false);
    const [errorMessage, setErrorMessage] = React.useState<String>("");

    React.useEffect(() => {
        if (errorMessage !== "") {
            setErrorMessage("");
        }
    }, [name, file]);

    const primaryLocationOptions: IDropdownOption[] = [
        { key: FileLocation.WestEurope, text: "West Europe" },
        { key: FileLocation.GermanyWestCentral, text: "Germany West Central" },
        { key: FileLocation.NorthEurope, text: "North Europe" }];

    const redundancyOptions: IDropdownOption[] = [
        { key: Redundancy.Locally, text: "Locally" },
        { key: Redundancy.Zone, text: "Zone" },
        { key: Redundancy.Globally, text: "Globally" },
        { key: Redundancy.Custom, text: "Custom" }];

    const onChangedLocation = (event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption): void => {
        if (option) {
            setLocation(option.key as FileLocation);
        }
    };

    const onChangedRedundancy = (event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption): void => {
        if (option) {
            setRedundancy(option.key as Redundancy);
        }
    };

    const onFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files && e.target.files[0];
        if (file) {
            setFile(file);
        }
    };

    const handleVersioningChange = (ev?: React.FormEvent<HTMLElement | HTMLInputElement> | undefined, checked?: boolean | undefined) => {
        setVersioning(checked || false);
    };

    const handleSaveFile = (): void => {
        let newErrorMessage: string = "";
        if (name === "") {
            newErrorMessage += "Name can't be empty.";
        }

        if (IsNullOrUndefined(file)) {
            newErrorMessage += " Select a file.";
        }

        if (IsNullOrUndefined(location)) {
            newErrorMessage += " Select a location.";
        }

        if (IsNullOrUndefined(redundancy)) {
            newErrorMessage += " Select a redundancy policy.";
        }

        if (newErrorMessage !== "") {
            setErrorMessage(newErrorMessage);
            return;
        }

        const newAppFile: ICreateFileDTO = {
            jwt: localStorage.getItem("jwt") as string,
            id: "00000000-0000-0000-0000-000000000000",
            name: name + "." + file?.name.split('.').pop(),
            location: location as FileLocation,
            redundancy: redundancy as Redundancy,
            versioning: versioning
        };

        const formData = new FormData();
        formData.append('dto', JSON.stringify(newAppFile))
        formData.append('file', file as File);

        AppFilesService.Addfile(formData)
            .then(function (response) {
                props.onAddedFile(response.data)
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
                    styles={titleInputStyles}
                    placeholder="Title"
                />
                {!IsNullOrUndefined(file) &&
                    <div style={{ marginLeft: "5px" }}>
                        {"." + file?.name.split('.').pop()}
                    </div>
                }
            </Stack>
            <Dropdown
                options={primaryLocationOptions}
                defaultSelectedKey={location}
                onChange={onChangedLocation}
                placeholder="Location"
            />
            <Dropdown
                options={redundancyOptions}
                defaultSelectedKey={redundancy}
                onChange={onChangedRedundancy}
                placeholder="Redundancy"
            />
            <Stack>
                <Label>File</Label>
                <input
                    type="file"
                    onChange={onFileChange}
                />
            </Stack>
            <Checkbox
                label="Support versioning"
                checked={versioning}
                onChange={handleVersioningChange}
            />
            <button className={buttonClassName} onClick={handleSaveFile}>
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