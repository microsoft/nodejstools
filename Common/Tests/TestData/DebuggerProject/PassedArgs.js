console.log("passed args");

if (42 != process.argv[2]) {
    throw new Error("Invalid args");
}
