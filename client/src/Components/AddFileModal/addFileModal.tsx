import { Checkbox, Dropdown, IDropdownOption, Icon, Label, Stack, Text, TextField } from "@fluentui/react";
import React from "react";
import { ICreateFileDTO } from "../../DTO/CreateFileDTO";
import { FileLocation } from "../../Enums/FileLocation";
import { Redundancy } from "../../Enums/Redundancy";
import { buttonClassName, iconClassName } from "../../Pages/Home/homePage.styles";
import { AppFilesService } from "../../services";
import { IsNullOrUndefined } from "../../utils";
import { errorMessageClassName, extenssionClassName, modalContainerClassName, nameStyles, versionNameStyles } from "./addFileModal.styles";
import { AddFileModalProps } from "./addFileModal.types";

export const AddFileModal = (props: AddFileModalProps): JSX.Element => {
    const [name, setName] = React.useState<string>("");
    const [file, setFile] = React.useState<File | null>(null);
    const [location, setLocation] = React.useState<FileLocation | null>(null);
    const [secondaryLocation, setSecondaryLocation] = React.useState<FileLocation | null>(null);
    const [redundancy, setRedundancy] = React.useState<Redundancy | null>(null);
    const [versioning, setVersioning] = React.useState<boolean>(false);
    const [errorMessage, setErrorMessage] = React.useState<string>("");
    const [versionFileName, setVersionFileName] = React.useState<string>("Original");

    React.useEffect(() => {
        if (errorMessage !== "") {
            setErrorMessage("");
        }
    }, [name, file, location, secondaryLocation, redundancy, versionFileName]);

    React.useEffect(() => {
        if (redundancy !== Redundancy.Custom) {
            setSecondaryLocation(null);
        }
    }, [redundancy]);

    const locationOptions: IDropdownOption[] = [
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

    const onChangedSecondaryLocation = (event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption): void => {
        if (option) {
            setSecondaryLocation(option.key as FileLocation);
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

        if (redundancy === Redundancy.Custom && IsNullOrUndefined(secondaryLocation)) {
            newErrorMessage += " Select a secondary location.";
        }

        if (versioning && IsNullOrUndefined(redundancy)) {
            newErrorMessage += " Version name can't be empty.";
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
            secondaryLocation: secondaryLocation as FileLocation,
            redundancy: redundancy as Redundancy,
            versioning: versioning,
            versionName: versionFileName
        };

        const formData = new FormData();
        formData.append('dto', JSON.stringify(newAppFile))
        formData.append('file', file as File);

        AppFilesService.AddFile(formData)
            .then(function (response) {
                props.onAddedFile(response.data)
            })
            .catch(function (error) {
                console.log(error);
            });
    };

    return (
        <Stack horizontal className={modalContainerClassName} tokens={{ childrenGap: 50 }}>
            <Stack style={{ width: '52%' }} verticalAlign="space-between">
                <Stack horizontal>
                    <TextField
                        value={name}
                        onChange={(event, newValue) => setName(newValue ?? "")}
                        styles={nameStyles}
                        label="Name"
                    />
                    {!IsNullOrUndefined(file) &&
                        <div className={extenssionClassName}>
                            {"." + file?.name.split('.').pop()}
                        </div>
                    }
                </Stack>
                <Dropdown
                    options={locationOptions.filter((option) => option.key !== secondaryLocation)}
                    defaultSelectedKey={location}
                    onChange={onChangedLocation}
                    label="Location"
                />
                <Dropdown
                    options={redundancyOptions}
                    defaultSelectedKey={redundancy}
                    onChange={onChangedRedundancy}
                    label="Redundancy"
                />
                {redundancy === Redundancy.Custom &&
                    <Dropdown
                        options={locationOptions.filter((option) => option.key !== location)}
                        defaultSelectedKey={secondaryLocation}
                        onChange={onChangedSecondaryLocation}
                        label="Secondary Location"
                    />
                }
                <Stack>
                    <Label>File</Label>
                    <input
                        type="file"
                        onChange={onFileChange}
                    />
                </Stack>
                <Stack>
                    <Checkbox
                        label="Support versioning"
                        checked={versioning}
                        onChange={handleVersioningChange}
                    />
                    <div style={versioning ? undefined : { height: "60px" }}>
                        {versioning &&
                            <TextField
                                value={versionFileName}
                                onChange={(event, newValue) => setVersionFileName(newValue ?? "")}
                                styles={versionNameStyles}
                                label="Version Name"
                            />
                        }
                    </div>
                </Stack>
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
            <Stack tokens={{ childrenGap: 20 }} style={{ marginTop: "10px", maxWidth: 700 }}>
                <Text variant="xLargePlus" styles={{ root: { fontWeight: 600, marginBottom: 16 } }}>
                    Storage Redundancy Options
                </Text>

                <Stack tokens={{ childrenGap: 8 }}>
                    <Text variant="large" styles={{ root: { fontWeight: 600 } }}>Locally Redundant Storage (LRS)</Text>
                    <Text variant="medium">
                        Keeps your data safe within a single data center. Best for low-cost and non-critical data.
                    </Text>
                </Stack>

                <Stack tokens={{ childrenGap: 8 }}>
                    <Text variant="large" styles={{ root: { fontWeight: 600 } }}>Zone-Redundant Storage (ZRS)</Text>
                    <Text variant="medium">
                        Protects your data across multiple availability zones in the same region. Ideal for high availability needs.
                    </Text>
                </Stack>

                <Stack tokens={{ childrenGap: 8 }}>
                    <Text variant="large" styles={{ root: { fontWeight: 600 } }}>Geo-Redundant Storage (GRS)</Text>
                    <Text variant="medium">
                        Replicates data to a secondary region hundreds of miles away for disaster recovery.
                    </Text>
                </Stack>

                <Stack tokens={{ childrenGap: 8 }}>
                    <Text variant="large" styles={{ root: { fontWeight: 600 } }}>Custom Redundancy</Text>
                    <Text variant="medium">
                        Lets you choose specific availability regions, creating two copies using LRS in those respective regions.
                    </Text>
                </Stack>
            </Stack>
        </Stack>
    )
}