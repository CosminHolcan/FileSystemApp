import React from "react";

export const authPageBackgroundStyle: React.CSSProperties = {
    backgroundColor: "#0078d4",
    height: "100vh",
};

export const authFormContainerStyle: React.CSSProperties = {
    backgroundColor: "white",
    borderRadius: "20px"
};

export const loginFormContainerStyle: React.CSSProperties = {
    ...authFormContainerStyle,
    height: "60vh",
    width: "30vw",
};

export const registerFormContainerStyle: React.CSSProperties = {
    ...authFormContainerStyle,
    height: "75vh",
    width: "30vw",
};

export const fieldContainerStyle: React.CSSProperties = {
    height: "5vh",
    width: "20vw",
    marginRight: "5vw",
    marginLeft: "5vw",
    marginTop: "5vh"
};

export const passwordFieldContainerStyle: React.CSSProperties = {
    ...fieldContainerStyle,
    marginTop: "8vh",
    marginBottom: "12vh"
};

export const repeatPasswordFieldContainerStyle: React.CSSProperties = {
    ...fieldContainerStyle,
    marginBottom: "10vh"
};

export const authLabelStyle: React.CSSProperties = {
    fontFamily: "Grotesco",
    fontSize: "20px"
};

export const authLabelSmallStyle: React.CSSProperties = {
    fontFamily: "Grotesco",
    fontSize: "16px"
};

export const authButtonStyle: React.CSSProperties = {
    borderRadius: "20px",
    borderWidth: 0,
    width: "10vw",
    height: "4vh",
    backgroundColor: "#0078d4",
    fontFamily: "Grotesco",
    fontSize: 15,
    color: "white",
    cursor: 'pointer'
};

export const authButtonLargeStyle: React.CSSProperties = {
    ...authButtonStyle,
    height: "6vh"
};

export const authButtonWithMarginStyle: React.CSSProperties = {
    ...authButtonStyle,
    marginRight: "2vw"
};

export const authButtonLargeWithMarginStyle: React.CSSProperties = {
    ...authButtonLargeStyle,
    marginRight: "2vw"
};

export const authErrorMessageStyle: React.CSSProperties = {
    fontFamily: "Grotesco",
    fontSize: 17,
    color: "red",
    marginLeft: "5vw",
    marginTop: "5vh"
};

export const authErrorMessageSmallStyle: React.CSSProperties = {
    fontFamily: "Grotesco",
    fontSize: 15,
    color: "red",
    marginLeft: "5vw",
    marginTop: "3vh"
};
