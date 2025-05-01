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