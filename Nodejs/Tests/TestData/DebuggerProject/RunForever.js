function RepeatAfter(i) {
    setTimeout(function () { RepeatAfter(i); }, i);
}
RepeatAfter(100);
