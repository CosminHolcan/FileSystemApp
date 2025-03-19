import { Label, PrimaryButton, Stack, TextField } from "@fluentui/react";
import React from "react";
import { IAppFileDTO } from "../../DTO/AppFileDTO";
import { AppFilesService } from "../../services";
import { modalContainerClassName } from "./homePage.styles";

export const HomePage = (): JSX.Element => {
    const [name, setName] = React.useState<string>("");
    const [file, setFile] = React.useState<File | null>(null);
    const [files, setFiles] = React.useState<IAppFileDTO[]>([]);

    React.useEffect(() => {
        AppFilesService.ReadFilesByUser({ jwt: localStorage.getItem("jwt") as string })
            .then(function (response) {
                setFiles(response.data);
            })
    }, []);

    const handleAddFile = (): void => {
        // const dtoData: ICreateFileDTO = {
        //     jwt: localStorage.getItem("jwt") as string,
        //     name: name,
        //     storageAccount:  'DefaultEndpointsProtocol=https;AccountName=fsawelrsnoversioning;AccountKey=nXXASVwJomA7qrN9gQW6T0ZXMzG3pPmPGZMnc+rcq6SKThy/Rtl7opeAd7YYns5moavQ5HPcqlCu+AStfi4X+g==;EndpointSuffix=core.windows.net'
        // }
        const formData = new FormData();
        formData.append('jwt', localStorage.getItem("jwt") as string);
        formData.append('name', name + "." + file?.name.split('.').pop());
        formData.append('storageAccount', 'DefaultEndpointsProtocol=https;AccountName=fsawelrsnoversioning;AccountKey=nXXASVwJomA7qrN9gQW6T0ZXMzG3pPmPGZMnc+rcq6SKThy/Rtl7opeAd7YYns5moavQ5HPcqlCu+AStfi4X+g==;EndpointSuffix=core.windows.net');
        formData.append('file', file as File);

        AppFilesService.Addfile(formData)
            .then(function (response) {
                const newFile: any = response.data;
                setFiles([...files, newFile])
            })
            .catch(function (error) {
                console.log(error);
            });
    };

    const onFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files && e.target.files[0];
        if (file) {
            setFile(file);
        }
    };

    return (
        <Stack className={modalContainerClassName}>
            <TextField
                label="Name"
                value={name}
                onChange={(event, newValue) => setName(newValue || '')}
            />
            <Label>File</Label>
            <input
                type="file"
                onChange={onFileChange}
            />
            <PrimaryButton style={{ marginTop: "20px" }} onClick={handleAddFile}>Save</PrimaryButton>

            <div>
                <h3>File List</h3>
                <ul>
                    {files.map((file) => (
                        <li key={file.id}>
                            <a href={"https://fsawelrsnoversioning.blob.core.windows.net/container/" + file.name} >
                                {file.name}
                            </a>
                            {/* You can add additional details here like storageAccount if needed */}
                        </li>
                    ))}
                </ul>
            </div>
        </Stack>
    )
};