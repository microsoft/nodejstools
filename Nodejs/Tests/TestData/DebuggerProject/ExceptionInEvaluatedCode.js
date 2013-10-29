try {
    var val = "";
    Function('', 'return (' + val + ');');
} catch (ex) {
    console.log('Caught exception');
}
console.log('Done');