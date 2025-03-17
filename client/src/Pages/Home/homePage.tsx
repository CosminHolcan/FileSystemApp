export const HomePage = (): JSX.Element => {
    return <div>
        Hello {localStorage.getItem("userName") as string}
    </div>
}