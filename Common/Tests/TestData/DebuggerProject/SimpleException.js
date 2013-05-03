try {
    var error = new Error("Error description");
    console.log("Raise: " + error);
    throw error;
} catch (exception) {
    console.log("Caught: " + exception);
}