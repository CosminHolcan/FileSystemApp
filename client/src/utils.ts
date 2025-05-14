import { FileLocation } from "./Enums/FileLocation";

export const dateAsString = (date: Date): string => {
    const monthNames: string[] = [
        "January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December"
    ];

    const day: string = String(date.getDate()).padStart(2, '0');
    const month: string = monthNames[date.getMonth()];
    const year: number = date.getFullYear();

    return `${day} ${month} ${year}`;
};

export const getDisplayStringLocation = (location: FileLocation): string => {
    switch (location) {
        case FileLocation.WestEurope: {
            return "West Europe";
        }
        case FileLocation.GermanyWestCentral: {
            return "Germany West Central";
        }
        case FileLocation.NorthEurope: {
            return "North Europe";
        }
        default: {
            return "";
        }
    }
};

export const IsNullOrUndefined = (object: any): boolean => {
    return object === null || object === undefined;
};

export const downloadBlobWithName = async (tokenSAS: string, desiredFileName: string) => {
    try {
        // const link = document.createElement('a');
        // link.href = tokenSAS;
        // link.download = desiredFileName;
        // document.body.appendChild(link);
        // link.click();
        // document.body.removeChild(link);

        const response: Response = await fetch(tokenSAS);

        if (!response.ok) {
            throw new Error(`Failed to fetch file: ${response.statusText}`);
        }

        const blob: Blob = await response.blob();

        const link: HTMLAnchorElement = document.createElement('a');
        link.href = window.URL.createObjectURL(blob);
        link.download = desiredFileName;

        document.body.appendChild(link);
        link.click();

        document.body.removeChild(link);
        window.URL.revokeObjectURL(link.href);
    }
    catch (error) {
        console.error('Download failed', error);
    }
};