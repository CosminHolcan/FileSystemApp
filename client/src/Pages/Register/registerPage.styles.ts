import React from "react";

export const RegisterContainerStyle: React.CSSProperties = {
    backgroundColor: "#0EBFE9",
    height: "100vh"
}

export const RegisterFormContainerStyle: React.CSSProperties = {
    backgroundColor: "white",
    height: "75vh",
    width: "30vw",
    borderRadius: "20px"
}

export const LabelStyle: React.CSSProperties = {
    fontFamily: "Grotesco",
    fontSize: "16px"
}

export const FirstNameContainerStyle: React.CSSProperties = {
    height: "5vh",
    width: "20vw",
    marginRight: "5vw",
    marginLeft: "5vw",
    marginTop: "5vh"
}

export const MiddleFieldContainerStyle: React.CSSProperties = {
    height: "5vh",
    width: "20vw",
    marginRight: "5vw",
    marginLeft: "5vw",
    marginTop: "5vh"
}

export const RepeatPasswordContainerStyle: React.CSSProperties = {
    height: "5vh",
    width: "20vw",
    marginRight: "5vw",
    marginLeft: "5vw",
    marginTop: "5vh",
    marginBottom: "10vh"
}

export const ButtonLoginStyle: React.CSSProperties = {
    borderRadius: "20px",
    borderWidth: 0,
    width: "10vw",
    height: "6vh",
    backgroundColor: "#0EBFE9",
    fontFamily: "Grotesco",
    fontSize: 15,
    color: "white"
}

export const ButtonRegisterStyle: React.CSSProperties = {
    ...ButtonLoginStyle,
    marginRight: "2vw"
}

export const ErrorMessageStyle: React.CSSProperties = {
    fontFamily: "Grotesco",
    fontSize: 15,
    color: "red",
    marginLeft: "5vw",
    marginTop: "3vh"
}