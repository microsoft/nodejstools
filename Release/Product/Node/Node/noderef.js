
function require(module) {
    switch (module) {
        case "timers": return new timers();
        case "module": return new module();
        case "addons": return new addons();
        case "util": return new util();
        case "Events": return new Events();
        case "domain": return new domain();
        case "buffer": return new buffer();
        case "stream": return new stream();
        case "crypto": return new crypto();
        case "tls_(ssl)": return new tls_(ssl)();
        case "stringdecoder": return new stringdecoder();
        case "fs": return new fs();
        case "path": return new path();
        case "net": return new net();
        case "dgram": return new dgram();
        case "dns": return new dns();
        case "http": return new http();
        case "https": return new https();
        case "url": return new url();
        case "querystring": return new querystring();
        case "punycode": return new punycode();
        case "readline": return new readline();
        case "repl": return new repl();
        case "vm": return new vm();
        case "child_process": return new child_process();
        case "assert": return new assert();
        case "tty": return new tty();
        case "zlib": return new zlib();
        case "os": return new os();
        case "cluster": return new cluster();
        // **NTVS** INSERT USER MODULE SWITCH HERE **NTVS**
    }
}

function timers() {
    /// <summary><p>All of the timer functions are globals.  You do not need to <code>require()</code>this module in order to use them.</p></summary>
    this.setTimeout = function(callback, delay, arg) {
        /// <summary><p>To schedule execution of a one-time <code>callback</code> after <code>delay</code> milliseconds. Returns a<code>timeoutId</code> for possible use with <code>clearTimeout()</code>. Optionally you canalso pass arguments to the callback.</p><p>It is important to note that your callback will probably not be called in exactly<code>delay</code> milliseconds - Node.js makes no guarantees about the exact timing of whenthe callback will fire, nor of the ordering things will fire in. The callback willbe called as close as possible to the time specified.</p></summary>
        /// <signature>
        /// <param name="callback"></param>
        /// <param name="delay"></param>
        /// <param name="arg"></param>
        /// <param name="..."></param>
        /// </signature>
    }
    this.clearTimeout = function(timeoutId) {
        /// <summary><p>Prevents a timeout from triggering.</p></summary>
        /// <signature>
        /// <param name="timeoutId"></param>
        /// </signature>
    }
    this.setInterval = function(callback, delay, arg) {
        /// <summary><p>To schedule the repeated execution of <code>callback</code> every <code>delay</code> milliseconds.Returns a <code>intervalId</code> for possible use with <code>clearInterval()</code>. Optionallyyou can also pass arguments to the callback.</p></summary>
        /// <signature>
        /// <param name="callback"></param>
        /// <param name="delay"></param>
        /// <param name="arg"></param>
        /// <param name="..."></param>
        /// </signature>
    }
    this.clearInterval = function(intervalId) {
        /// <summary><p>Stops a interval from triggering.</p></summary>
        /// <signature>
        /// <param name="intervalId"></param>
        /// </signature>
    }
} // module timers

function module() {
    /// <summary><p>Node has a simple module loading system.  In Node, files and modules are inone-to-one correspondence.  As an example, <code>foo.js</code> loads the module<code>circle.js</code> in the same directory.</p><p>The contents of <code>foo.js</code>:</p><pre><code>var circle = require(&#39;./circle.js&#39;);console.log( &#39;The area of a circle of radius 4 is &#39;           + circle.area(4));</code></pre><p>The contents of <code>circle.js</code>:</p><pre><code>var PI = Math.PI;exports.area = function (r) {  return PI * r * r;};exports.circumference = function (r) {  return 2 * PI * r;};</code></pre><p>The module <code>circle.js</code> has exported the functions <code>area()</code> and<code>circumference()</code>.  To export an object, add to the special <code>exports</code>object.</p><p>Variableslocal to the module will be private. In this example the variable <code>PI</code> isprivate to <code>circle.js</code>.</p><p>The module system is implemented in the <code>require(&quot;module&quot;)</code> module.</p></summary>
} // module module

function addons() {
    /// <summary><p>Addons are dynamically linked shared objects. They can provide glue to C andC++ libraries. The API (at the moment) is rather complex, involvingknowledge of several libraries:</p><ul><li><p>V8 JavaScript, a C++ library. Used for interfacing with JavaScript:creating objects, calling functions, etc.  Documented mostly in the<code>v8.h</code> header file (<code>deps/v8/include/v8.h</code> in the Node source tree),which is also available <a href="http://izs.me/v8-docs/main.html">online</a>.</p></li><li><p><a href="https://github.com/joyent/libuv">libuv</a>, C event loop library. Anytime oneneeds to wait for a file descriptor to become readable, wait for a timer, orwait for a signal to received one will need to interface with libuv. That is,if you perform any I/O, libuv will need to be used.</p></li><li><p>Internal Node libraries. Most importantly is the <code>node::ObjectWrap</code>class which you will likely want to derive from.</p></li><li><p>Others. Look in <code>deps/</code> for what else is available.</p></li></ul><p>Node statically compiles all its dependencies into the executable. Whencompiling your module, you don&#39;t need to worry about linking to any of theselibraries.</p></summary>
} // module addons

function util() {
    /// <summary><p>These functions are in the module <code>&#39;util&#39;</code>. Use <code>require(&#39;util&#39;)</code> to accessthem.</p></summary>
    this.format = function(format) {
        /// <summary><p>Returns a formatted string using the first argument as a <code>printf</code>-like format.</p><p>The first argument is a string that contains zero or more <em>placeholders</em>.Each placeholder is replaced with the converted value from its correspondingargument. Supported placeholders are:</p><ul><li><code>%s</code> - String.</li><li><code>%d</code> - Number (both integer and float).</li><li><code>%j</code> - JSON.</li><li><code>%</code> - single percent sign (<code>&#39;%&#39;</code>). This does not consume an argument.</li></ul><p>If the placeholder does not have a corresponding argument, the placeholder isnot replaced.</p><pre><code>util.format(&#39;%s:%s&#39;, &#39;foo&#39;); // &#39;foo:%s&#39;</code></pre><p>If there are more arguments than placeholders, the extra arguments areconverted to strings with <code>util.inspect()</code> and these strings are concatenated,delimited by a space.</p><pre><code>util.format(&#39;%s:%s&#39;, &#39;foo&#39;, &#39;bar&#39;, &#39;baz&#39;); // &#39;foo:bar baz&#39;</code></pre><p>If the first argument is not a format string then <code>util.format()</code> returnsa string that is the concatenation of all its arguments separated by spaces.Each argument is converted to a string with <code>util.inspect()</code>.</p><pre><code>util.format(1, 2, 3); // &#39;1 2 3&#39;</code></pre></summary>
        /// <signature>
        /// <param name="format"></param>
        /// <param name="..."></param>
        /// </signature>
    }
    this.debug = function(string) {
        /// <summary><p>A synchronous output function. Will block the process andoutput <code>string</code> immediately to <code>stderr</code>.</p><pre><code>require(&#39;util&#39;).debug(&#39;message on stderr&#39;);</code></pre></summary>
        /// <signature>
        /// <param name="string"></param>
        /// </signature>
    }
    this.error = function() {
        /// <summary><p>Same as <code>util.debug()</code> except this will output all arguments immediately to<code>stderr</code>.</p></summary>
        /// <signature>
        /// <param name="..."></param>
        /// </signature>
    }
    this.puts = function() {
        /// <summary><p>A synchronous output function. Will block the process and output all argumentsto <code>stdout</code> with newlines after each argument.</p></summary>
        /// <signature>
        /// <param name="..."></param>
        /// </signature>
    }
    this.print = function() {
        /// <summary><p>A synchronous output function. Will block the process, cast each argument to astring then output to <code>stdout</code>. Does not place newlines after each argument.</p></summary>
        /// <signature>
        /// <param name="..."></param>
        /// </signature>
    }
    this.log = function(string) {
        /// <summary><p>Output with timestamp on <code>stdout</code>.</p><pre><code>require(&#39;util&#39;).log(&#39;Timestamped message.&#39;);</code></pre></summary>
        /// <signature>
        /// <param name="string"></param>
        /// </signature>
    }
    this.inspect = function(object, showHidden, depth, colors) {
        /// <summary><p>Return a string representation of <code>object</code>, which is useful for debugging.</p><p>If <code>showHidden</code> is <code>true</code>, then the object&#39;s non-enumerable properties will beshown too. Defaults to <code>false</code>.</p><p>If <code>depth</code> is provided, it tells <code>inspect</code> how many times to recurse whileformatting the object. This is useful for inspecting large complicated objects.</p><p>The default is to only recurse twice.  To make it recurse indefinitely, passin <code>null</code> for <code>depth</code>.</p><p>If <code>colors</code> is <code>true</code>, the output will be styled with ANSI color codes.Defaults to <code>false</code>.</p><p>Example of inspecting all properties of the <code>util</code> object:</p><pre><code>var util = require(&#39;util&#39;);console.log(util.inspect(util, true, null));</code></pre><p>Objects also may define their own <code>inspect(depth)</code> function which <code>util.inspect()</code>will invoke and use the result of when inspecting the object:</p><pre><code>var util = require(&#39;util&#39;);var obj = { name: &#39;nate&#39; };obj.inspect = function(depth) {  return &#39;{&#39; + this.name + &#39;}&#39;;};util.inspect(obj);  // &quot;{nate}&quot;</code></pre></summary>
        /// <signature>
        /// <param name="object"></param>
        /// <param name="showHidden"></param>
        /// <param name="depth"></param>
        /// <param name="colors"></param>
        /// </signature>
    }
    this.isArray = function(object) {
        /// <summary><p>Returns <code>true</code> if the given &quot;object&quot; is an <code>Array</code>. <code>false</code> otherwise.</p><pre><code>var util = require(&#39;util&#39;);util.isArray([])  // trueutil.isArray(new Array)  // trueutil.isArray({})  // false</code></pre></summary>
        /// <signature>
        /// <param name="object"></param>
        /// </signature>
    }
    this.isRegExp = function(object) {
        /// <summary><p>Returns <code>true</code> if the given &quot;object&quot; is a <code>RegExp</code>. <code>false</code> otherwise.</p><pre><code>var util = require(&#39;util&#39;);util.isRegExp(/some regexp/)  // trueutil.isRegExp(new RegExp(&#39;another regexp&#39;))  // trueutil.isRegExp({})  // false</code></pre></summary>
        /// <signature>
        /// <param name="object"></param>
        /// </signature>
    }
    this.isDate = function(object) {
        /// <summary><p>Returns <code>true</code> if the given &quot;object&quot; is a <code>Date</code>. <code>false</code> otherwise.</p><pre><code>var util = require(&#39;util&#39;);util.isDate(new Date())  // trueutil.isDate(Date())  // false (without &#39;new&#39; returns a String)util.isDate({})  // false</code></pre></summary>
        /// <signature>
        /// <param name="object"></param>
        /// </signature>
    }
    this.isError = function(object) {
        /// <summary><p>Returns <code>true</code> if the given &quot;object&quot; is an <code>Error</code>. <code>false</code> otherwise.</p><pre><code>var util = require(&#39;util&#39;);util.isError(new Error())  // trueutil.isError(new TypeError())  // trueutil.isError({ name: &#39;Error&#39;, message: &#39;an error occurred&#39; })  // false</code></pre></summary>
        /// <signature>
        /// <param name="object"></param>
        /// </signature>
    }
    this.pump = function(readableStream, writableStream, callback) {
        /// <summary><p>Read the data from <code>readableStream</code> and send it to the <code>writableStream</code>.When <code>writableStream.write(data)</code> returns <code>false</code> <code>readableStream</code> will bepaused until the <code>drain</code> event occurs on the <code>writableStream</code>. <code>callback</code> getsan error as its only argument and is called when <code>writableStream</code> is closed orwhen an error occurs.</p></summary>
        /// <signature>
        /// <param name="readableStream"></param>
        /// <param name="writableStream"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.inherits = function(constructor, superConstructor) {
        /// <summary><p>Inherit the prototype methods from one<a href="https://developer.mozilla.org/en/JavaScript/Reference/Global_Objects/Object/constructor">constructor</a>into another.  The prototype of <code>constructor</code> will be set to a newobject created from <code>superConstructor</code>.</p><p>As an additional convenience, <code>superConstructor</code> will be accessiblethrough the <code>constructor.super_</code> property.</p><pre><code>var util = require(&quot;util&quot;);var events = require(&quot;events&quot;);function MyStream() {    events.EventEmitter.call(this);}util.inherits(MyStream, events.EventEmitter);MyStream.prototype.write = function(data) {    this.emit(&quot;data&quot;, data);}var stream = new MyStream();console.log(stream instanceof events.EventEmitter); // trueconsole.log(MyStream.super_ === events.EventEmitter); // truestream.on(&quot;data&quot;, function(data) {    console.log(&#39;Received data: &quot;&#39; + data + &#39;&quot;&#39;);})stream.write(&quot;It works!&quot;); // Received data: &quot;It works!&quot;</code></pre></summary>
        /// <signature>
        /// <param name="constructor"></param>
        /// <param name="superConstructor"></param>
        /// </signature>
    }
} // module util

function Events() {
    /// <summary><p>Many objects in Node emit events: a <code>net.Server</code> emits an event each timea peer connects to it, a <code>fs.readStream</code> emits an event when the file isopened. All objects which emit events are instances of <code>events.EventEmitter</code>.You can access this module by doing: <code>require(&quot;events&quot;);</code></p><p>Typically, event names are represented by a camel-cased string, however,there aren&#39;t any strict restrictions on that, as any string will be accepted.</p><p>Functions can then be attached to objects, to be executed when an eventis emitted. These functions are called <em>listeners</em>.</p></summary>
    this.EventEmitter = function() {
        this.addListener = function(event, listener) {
            /// <summary><p>Adds a listener to the end of the listeners array for the specified event.</p><pre><code>server.on(&#39;connection&#39;, function (stream) {  console.log(&#39;someone connected!&#39;);});</code></pre></summary>
            /// <signature>
            /// <param name="event"></param>
            /// <param name="listener"></param>
            /// </signature>
            /// <signature>
            /// <param name="event"></param>
            /// <param name="listener"></param>
            /// </signature>
        }
        this.on = function(event, listener) {
            /// <summary><p>Adds a listener to the end of the listeners array for the specified event.</p><pre><code>server.on(&#39;connection&#39;, function (stream) {  console.log(&#39;someone connected!&#39;);});</code></pre></summary>
            /// <signature>
            /// <param name="event"></param>
            /// <param name="listener"></param>
            /// </signature>
        }
        this.once = function(event, listener) {
            /// <summary><p>Adds a <strong>one time</strong> listener for the event. This listener isinvoked only the next time the event is fired, after whichit is removed.</p><pre><code>server.once(&#39;connection&#39;, function (stream) {  console.log(&#39;Ah, we have our first user!&#39;);});</code></pre></summary>
            /// <signature>
            /// <param name="event"></param>
            /// <param name="listener"></param>
            /// </signature>
        }
        this.removeListener = function(event, listener) {
            /// <summary><p>Remove a listener from the listener array for the specified event.<strong>Caution</strong>: changes array indices in the listener array behind the listener.</p><pre><code>var callback = function(stream) {  console.log(&#39;someone connected!&#39;);};server.on(&#39;connection&#39;, callback);// ...server.removeListener(&#39;connection&#39;, callback);</code></pre></summary>
            /// <signature>
            /// <param name="event"></param>
            /// <param name="listener"></param>
            /// </signature>
        }
        this.removeAllListeners = function(event) {
            /// <summary><p>Removes all listeners, or those of the specified event.</p><p>Note that this will <strong>invalidate</strong> any arrays that have previously beenreturned by <code>emitter.listeners(event)</code>.</p></summary>
            /// <signature>
            /// <param name="event"></param>
            /// </signature>
        }
        this.setMaxListeners = function(n) {
            /// <summary><p>By default EventEmitters will print a warning if more than 10 listeners areadded for a particular event. This is a useful default which helps finding memory leaks.Obviously not all Emitters should be limited to 10. This function allowsthat to be increased. Set to zero for unlimited.</p></summary>
            /// <signature>
            /// <param name="n"></param>
            /// </signature>
        }
        this.listeners = function(event) {
            /// <summary><p>Returns an array of listeners for the specified event.</p><pre><code>server.on(&#39;connection&#39;, function (stream) {  console.log(&#39;someone connected!&#39;);});console.log(util.inspect(server.listeners(&#39;connection&#39;))); // [ [Function] ]</code></pre><p>This array <strong>may</strong> be a mutable reference to the same underlying list oflisteners that is used by the event subsystem.  However, certainactions (specifically, removeAllListeners) will invalidate thisreference.</p><p>If you would like to get a copy of the listeners at a specific point intime that is guaranteed not to change, make a copy, for example by doing<code>emitter.listeners(event).slice(0)</code>.</p><p>In a future release of node, this behavior <strong>may</strong> change to alwaysreturn a copy, for consistency.  In your programs, please do not rely onbeing able to modify the EventEmitter listeners using array methods.Always use the &#39;on&#39; method to add new listeners.</p></summary>
            /// <signature>
            /// <param name="event"></param>
            /// </signature>
        }
        this.emit = function(event, arg1, arg2) {
            /// <summary><p>Execute each of the listeners in order with the supplied arguments.</p></summary>
            /// <signature>
            /// <param name="event"></param>
            /// <param name="arg1"></param>
            /// <param name="arg2"></param>
            /// <param name="..."></param>
            /// </signature>
        }
        emitter = new Events().EventEmitter;
        /// <field name='newListener'><p>This event is emitted any time someone adds a new listener.</p></field>
        this.newListener = new emitter();
    }
} // module Events

function domain() {
    /// <summary><p>Domains provide a way to handle multiple different IO operations as asingle group.  If any of the event emitters or callbacks registered to adomain emit an <code>error</code> event, or throw an error, then the domain objectwill be notified, rather than losing the context of the error in the<code>process.on(&#39;uncaughtException&#39;)</code> handler, or causing the program toexit with an error code.</p><p>This feature is new in Node version 0.8.  It is a first pass, and isexpected to change significantly in future versions.  Please use it andprovide feedback.</p><p>Due to their experimental nature, the Domains features are disabled unlessthe <code>domain</code> module is loaded at least once.  No domains are created orregistered by default.  This is by design, to prevent adverse effects oncurrent programs.  It is expected to be enabled by default in futureNode.js versions.</p></summary>
    this.create = function() {
        /// <summary><p>Returns a new Domain object.</p></summary>
    }
    this.Domain = function() {
        this.run = function(fn) {
            /// <summary><p>Run the supplied function in the context of the domain, implicitlybinding all event emitters, timers, and lowlevel requests that arecreated in that context.</p><p>This is the most basic way to use a domain.</p><p>Example:</p><pre><code>var d = domain.create();d.on(&#39;error&#39;, function(er) {  console.error(&#39;Caught error!&#39;, er);});d.run(function() {  process.nextTick(function() {    setTimeout(function() { // simulating some various async stuff      fs.open(&#39;non-existent file&#39;, &#39;r&#39;, function(er, fd) {        if (er) throw er;        // proceed...      });    }, 100);  });});</code></pre><p>In this example, the <code>d.on(&#39;error&#39;)</code> handler will be triggered, ratherthan crashing the program.</p></summary>
            /// <signature>
            /// <param name="fn" type="Function"></param>
            /// </signature>
            /// <signature>
            /// <param name="fn"></param>
            /// </signature>
        }
        this.add = function(emitter) {
            /// <summary><p>Explicitly adds an emitter to the domain.  If any event handlers called bythe emitter throw an error, or if the emitter emits an <code>error</code> event, itwill be routed to the domain&#39;s <code>error</code> event, just like with implicitbinding.</p><p>This also works with timers that are returned from <code>setInterval</code> and<code>setTimeout</code>.  If their callback function throws, it will be caught bythe domain &#39;error&#39; handler.</p><p>If the Timer or EventEmitter was already bound to a domain, it is removedfrom that one, and bound to this one instead.</p></summary>
            /// <signature>
            /// <param name="emitter" type="EventEmitter">emitter or timer to be added to the domain</param>
            /// </signature>
            /// <signature>
            /// <param name="emitter"></param>
            /// </signature>
        }
        this.remove = function(emitter) {
            /// <summary><p>The opposite of <code>domain.add(emitter)</code>.  Removes domain handling from thespecified emitter.</p></summary>
            /// <signature>
            /// <param name="emitter" type="EventEmitter">emitter or timer to be removed from the domain</param>
            /// </signature>
            /// <signature>
            /// <param name="emitter"></param>
            /// </signature>
        }
        this.bind = function(callback) {
            /// <summary><p>The returned function will be a wrapper around the supplied callbackfunction.  When the returned function is called, any errors that arethrown will be routed to the domain&#39;s <code>error</code> event.</p><h4>Example</h4><pre><code>var d = domain.create();function readSomeFile(filename, cb) {  fs.readFile(filename, &#39;utf8&#39;, d.bind(function(er, data) {    // if this throws, it will also be passed to the domain    return cb(er, data ? JSON.parse(data) : null);  }));}d.on(&#39;error&#39;, function(er) {  // an error occurred somewhere.  // if we throw it now, it will crash the program  // with the normal line number and stack message.});</code></pre></summary>
            /// <signature>
            /// <param name="callback" type="Function">The callback function</param>
            /// <returns type="Function">The bound function</returns>
            /// </signature>
            /// <signature>
            /// <param name="callback"></param>
            /// </signature>
        }
        this.intercept = function(callback) {
            /// <summary><p>This method is almost identical to <code>domain.bind(callback)</code>.  However, inaddition to catching thrown errors, it will also intercept <code>Error</code>objects sent as the first argument to the function.</p><p>In this way, the common <code>if (er) return callback(er);</code> pattern can be replacedwith a single error handler in a single place.</p><h4>Example</h4><pre><code>var d = domain.create();function readSomeFile(filename, cb) {  fs.readFile(filename, &#39;utf8&#39;, d.intercept(function(data) {    // note, the first argument is never passed to the    // callback since it is assumed to be the &#39;Error&#39; argument    // and thus intercepted by the domain.    // if this throws, it will also be passed to the domain    // so the error-handling logic can be moved to the &#39;error&#39;    // event on the domain instead of being repeated throughout    // the program.    return cb(null, JSON.parse(data));  }));}d.on(&#39;error&#39;, function(er) {  // an error occurred somewhere.  // if we throw it now, it will crash the program  // with the normal line number and stack message.});</code></pre></summary>
            /// <signature>
            /// <param name="callback" type="Function">The callback function</param>
            /// <returns type="Function">The intercepted function</returns>
            /// </signature>
            /// <signature>
            /// <param name="callback"></param>
            /// </signature>
        }
        this.dispose = function() {
            /// <summary><p>The dispose method destroys a domain, and makes a best effort attempt toclean up any and all IO that is associated with the domain.  Streams areaborted, ended, closed, and/or destroyed.  Timers are cleared.Explicitly bound callbacks are no longer called.  Any error events thatare raised as a result of this are ignored.</p><p>The intention of calling <code>dispose</code> is generally to prevent cascadingerrors when a critical part of the Domain context is found to be in anerror state.</p><p>Once the domain is disposed the <code>dispose</code> event will emit.</p><p>Note that IO might still be performed.  However, to the highest degreepossible, once a domain is disposed, further errors from the emitters inthat set will be ignored.  So, even if some remaining actions are stillin flight, Node.js will not communicate further about them.</p></summary>
        }
        /// <field name='members'><p>An array of timers and event emitters that have been explicitly addedto the domain.</p></field>
        this.members = undefined;
    }
} // module domain

function buffer() {
    /// <summary><p>Pure JavaScript is Unicode friendly but not nice to binary data.  Whendealing with TCP streams or the file system, it&#39;s necessary to handle octetstreams. Node has several strategies for manipulating, creating, andconsuming octet streams.</p><p>Raw data is stored in instances of the <code>Buffer</code> class. A <code>Buffer</code> is similarto an array of integers but corresponds to a raw memory allocation outsidethe V8 heap. A <code>Buffer</code> cannot be resized.</p><p>The <code>Buffer</code> class is a global, making it very rare that one would needto ever <code>require(&#39;buffer&#39;)</code>.</p><p>Converting between Buffers and JavaScript string objects requires an explicitencoding method.  Here are the different string encodings.</p><ul><li><p><code>&#39;ascii&#39;</code> - for 7 bit ASCII data only.  This encoding method is very fast, andwill strip the high bit if set.Note that this encoding converts a null character (<code>&#39;\0&#39;</code> or <code>&#39;\u0000&#39;</code>) into<code>0x20</code> (character code of a space). If you want to convert a null characterinto <code>0x00</code>, you should use <code>&#39;utf8&#39;</code>.</p></li><li><p><code>&#39;utf8&#39;</code> - Multibyte encoded Unicode characters. Many web pages and otherdocument formats use UTF-8.</p></li><li><p><code>&#39;utf16le&#39;</code> - 2 or 4 bytes, little endian encoded Unicode characters.Surrogate pairs (U+10000 to U+10FFFF) are supported.</p></li><li><p><code>&#39;ucs2&#39;</code> - Alias of <code>&#39;utf16le&#39;</code>.</p></li><li><p><code>&#39;base64&#39;</code> - Base64 string encoding.</p></li><li><p><code>&#39;binary&#39;</code> - A way of encoding raw binary data into strings by using onlythe first 8 bits of each character. This encoding method is deprecated andshould be avoided in favor of <code>Buffer</code> objects where possible. This encodingwill be removed in future versions of Node.</p></li><li><p><code>&#39;hex&#39;</code> - Encode each byte as two hexadecimal characters.</p></li></ul></summary>
    this.Buffer = function() {
        this.write = function(string, offset, length, encoding) {
            /// <summary><p>Writes <code>string</code> to the buffer at <code>offset</code> using the given encoding.<code>offset</code> defaults to <code>0</code>, <code>encoding</code> defaults to <code>&#39;utf8&#39;</code>. <code>length</code> isthe number of bytes to write. Returns number of octets written. If <code>buffer</code> didnot contain enough space to fit the entire string, it will write a partialamount of the string. <code>length</code> defaults to <code>buffer.length - offset</code>.The method will not write partial characters.</p><pre><code>buf = new Buffer(256);len = buf.write(&#39;\u00bd + \u00bc = \u00be&#39;, 0);console.log(len + &quot; bytes: &quot; + buf.toString(&#39;utf8&#39;, 0, len));</code></pre><p>The number of characters written (which may be different than the number ofbytes written) is set in <code>Buffer._charsWritten</code> and will be overwritten thenext time <code>buf.write()</code> is called.</p></summary>
            /// <signature>
            /// <param name="string">String - data to be written to buffer</param>
            /// <param name="offset">Number, Optional, Default: 0</param>
            /// <param name="length">Number, Optional, Default: `buffer.length - offset`</param>
            /// <param name="encoding">String, Optional, Default: 'utf8'</param>
            /// </signature>
            /// <signature>
            /// <param name="string"></param>
            /// <param name="offset"></param>
            /// <param name="length"></param>
            /// <param name="encoding"></param>
            /// </signature>
        }
        this.toString = function(encoding, start, end) {
            /// <summary><p>Decodes and returns a string from buffer data encoded with <code>encoding</code>(defaults to <code>&#39;utf8&#39;</code>) beginning at <code>start</code> (defaults to <code>0</code>) and ending at<code>end</code> (defaults to <code>buffer.length</code>).</p><p>See <code>buffer.write()</code> example, above.</p></summary>
            /// <signature>
            /// <param name="encoding">String, Optional, Default: 'utf8'</param>
            /// <param name="start">Number, Optional, Default: 0</param>
            /// <param name="end">Number, Optional, Default: `buffer.length`</param>
            /// </signature>
            /// <signature>
            /// <param name="encoding"></param>
            /// <param name="start"></param>
            /// <param name="end"></param>
            /// </signature>
        }
        this.copy = function(targetBuffer, targetStart, sourceStart, sourceEnd) {
            /// <summary><p>Does copy between buffers. The source and target regions can be overlapped.<code>targetStart</code> and <code>sourceStart</code> default to <code>0</code>.<code>sourceEnd</code> defaults to <code>buffer.length</code>.</p><p>Example: build two Buffers, then copy <code>buf1</code> from byte 16 through byte 19into <code>buf2</code>, starting at the 8th byte in <code>buf2</code>.</p><pre><code>buf1 = new Buffer(26);buf2 = new Buffer(26);for (var i = 0 ; i &lt; 26 ; i++) {  buf1[i] = i + 97; // 97 is ASCII a  buf2[i] = 33; // ASCII !}buf1.copy(buf2, 8, 16, 20);console.log(buf2.toString(&#39;ascii&#39;, 0, 25));// !!!!!!!!qrst!!!!!!!!!!!!!</code></pre></summary>
            /// <signature>
            /// <param name="targetBuffer">Buffer object - Buffer to copy into</param>
            /// <param name="targetStart">Number, Optional, Default: 0</param>
            /// <param name="sourceStart">Number, Optional, Default: 0</param>
            /// <param name="sourceEnd">Number, Optional, Default: `buffer.length`</param>
            /// </signature>
            /// <signature>
            /// <param name="targetBuffer"></param>
            /// <param name="targetStart"></param>
            /// <param name="sourceStart"></param>
            /// <param name="sourceEnd"></param>
            /// </signature>
        }
        this.slice = function(start, end) {
            /// <summary><p>Returns a new buffer which references the same memory as the old, but offsetand cropped by the <code>start</code> (defaults to <code>0</code>) and <code>end</code> (defaults to<code>buffer.length</code>) indexes.</p><p><strong>Modifying the new buffer slice will modify memory in the original buffer!</strong></p><p>Example: build a Buffer with the ASCII alphabet, take a slice, then modify onebyte from the original Buffer.</p><pre><code>var buf1 = new Buffer(26);for (var i = 0 ; i &lt; 26 ; i++) {  buf1[i] = i + 97; // 97 is ASCII a}var buf2 = buf1.slice(0, 3);console.log(buf2.toString(&#39;ascii&#39;, 0, buf2.length));buf1[0] = 33;console.log(buf2.toString(&#39;ascii&#39;, 0, buf2.length));// abc// !bc</code></pre></summary>
            /// <signature>
            /// <param name="start">Number, Optional, Default: 0</param>
            /// <param name="end">Number, Optional, Default: `buffer.length`</param>
            /// </signature>
            /// <signature>
            /// <param name="start"></param>
            /// <param name="end"></param>
            /// </signature>
        }
        this.readUInt8 = function(offset, noAssert) {
            /// <summary><p>Reads an unsigned 8 bit integer from the buffer at the specified offset.</p><p>Set <code>noAssert</code> to true to skip validation of <code>offset</code>. This means that <code>offset</code>may be beyond the end of the buffer. Defaults to <code>false</code>.</p><p>Example:</p><pre><code>var buf = new Buffer(4);buf[0] = 0x3;buf[1] = 0x4;buf[2] = 0x23;buf[3] = 0x42;for (ii = 0; ii &lt; buf.length; ii++) {  console.log(buf.readUInt8(ii));}// 0x3// 0x4// 0x23// 0x42</code></pre></summary>
            /// <signature>
            /// <param name="offset">Number</param>
            /// <param name="noAssert">Boolean, Optional, Default: false</param>
            /// <returns>Number</returns>
            /// </signature>
            /// <signature>
            /// <param name="offset"></param>
            /// <param name="noAssert"></param>
            /// </signature>
        }
        this.readUInt16LE = function(offset, noAssert) {
            /// <summary><p>Reads an unsigned 16 bit integer from the buffer at the specified offset withspecified endian format.</p><p>Set <code>noAssert</code> to true to skip validation of <code>offset</code>. This means that <code>offset</code>may be beyond the end of the buffer. Defaults to <code>false</code>.</p><p>Example:</p><pre><code>var buf = new Buffer(4);buf[0] = 0x3;buf[1] = 0x4;buf[2] = 0x23;buf[3] = 0x42;console.log(buf.readUInt16BE(0));console.log(buf.readUInt16LE(0));console.log(buf.readUInt16BE(1));console.log(buf.readUInt16LE(1));console.log(buf.readUInt16BE(2));console.log(buf.readUInt16LE(2));// 0x0304// 0x0403// 0x0423// 0x2304// 0x2342// 0x4223</code></pre></summary>
            /// <signature>
            /// <param name="offset">Number</param>
            /// <param name="noAssert">Boolean, Optional, Default: false</param>
            /// <returns>Number</returns>
            /// </signature>
            /// <signature>
            /// <param name="offset"></param>
            /// <param name="noAssert"></param>
            /// </signature>
            /// <signature>
            /// <param name="offset"></param>
            /// <param name="noAssert"></param>
            /// </signature>
        }
        this.readUInt16BE = function(offset, noAssert) {
            /// <summary><p>Reads an unsigned 16 bit integer from the buffer at the specified offset withspecified endian format.</p><p>Set <code>noAssert</code> to true to skip validation of <code>offset</code>. This means that <code>offset</code>may be beyond the end of the buffer. Defaults to <code>false</code>.</p><p>Example:</p><pre><code>var buf = new Buffer(4);buf[0] = 0x3;buf[1] = 0x4;buf[2] = 0x23;buf[3] = 0x42;console.log(buf.readUInt16BE(0));console.log(buf.readUInt16LE(0));console.log(buf.readUInt16BE(1));console.log(buf.readUInt16LE(1));console.log(buf.readUInt16BE(2));console.log(buf.readUInt16LE(2));// 0x0304// 0x0403// 0x0423// 0x2304// 0x2342// 0x4223</code></pre></summary>
            /// <signature>
            /// <param name="offset">Number</param>
            /// <param name="noAssert">Boolean, Optional, Default: false</param>
            /// <returns>Number</returns>
            /// </signature>
            /// <signature>
            /// <param name="offset"></param>
            /// <param name="noAssert"></param>
            /// </signature>
        }
        this.readUInt32LE = function(offset, noAssert) {
            /// <summary><p>Reads an unsigned 32 bit integer from the buffer at the specified offset withspecified endian format.</p><p>Set <code>noAssert</code> to true to skip validation of <code>offset</code>. This means that <code>offset</code>may be beyond the end of the buffer. Defaults to <code>false</code>.</p><p>Example:</p><pre><code>var buf = new Buffer(4);buf[0] = 0x3;buf[1] = 0x4;buf[2] = 0x23;buf[3] = 0x42;console.log(buf.readUInt32BE(0));console.log(buf.readUInt32LE(0));// 0x03042342// 0x42230403</code></pre></summary>
            /// <signature>
            /// <param name="offset">Number</param>
            /// <param name="noAssert">Boolean, Optional, Default: false</param>
            /// <returns>Number</returns>
            /// </signature>
            /// <signature>
            /// <param name="offset"></param>
            /// <param name="noAssert"></param>
            /// </signature>
            /// <signature>
            /// <param name="offset"></param>
            /// <param name="noAssert"></param>
            /// </signature>
        }
        this.readUInt32BE = function(offset, noAssert) {
            /// <summary><p>Reads an unsigned 32 bit integer from the buffer at the specified offset withspecified endian format.</p><p>Set <code>noAssert</code> to true to skip validation of <code>offset</code>. This means that <code>offset</code>may be beyond the end of the buffer. Defaults to <code>false</code>.</p><p>Example:</p><pre><code>var buf = new Buffer(4);buf[0] = 0x3;buf[1] = 0x4;buf[2] = 0x23;buf[3] = 0x42;console.log(buf.readUInt32BE(0));console.log(buf.readUInt32LE(0));// 0x03042342// 0x42230403</code></pre></summary>
            /// <signature>
            /// <param name="offset">Number</param>
            /// <param name="noAssert">Boolean, Optional, Default: false</param>
            /// <returns>Number</returns>
            /// </signature>
            /// <signature>
            /// <param name="offset"></param>
            /// <param name="noAssert"></param>
            /// </signature>
        }
        this.readInt8 = function(offset, noAssert) {
            /// <summary><p>Reads a signed 8 bit integer from the buffer at the specified offset.</p><p>Set <code>noAssert</code> to true to skip validation of <code>offset</code>. This means that <code>offset</code>may be beyond the end of the buffer. Defaults to <code>false</code>.</p><p>Works as <code>buffer.readUInt8</code>, except buffer contents are treated as two&#39;scomplement signed values.</p></summary>
            /// <signature>
            /// <param name="offset">Number</param>
            /// <param name="noAssert">Boolean, Optional, Default: false</param>
            /// <returns>Number</returns>
            /// </signature>
            /// <signature>
            /// <param name="offset"></param>
            /// <param name="noAssert"></param>
            /// </signature>
        }
        this.readInt16LE = function(offset, noAssert) {
            /// <summary><p>Reads a signed 16 bit integer from the buffer at the specified offset withspecified endian format.</p><p>Set <code>noAssert</code> to true to skip validation of <code>offset</code>. This means that <code>offset</code>may be beyond the end of the buffer. Defaults to <code>false</code>.</p><p>Works as <code>buffer.readUInt16*</code>, except buffer contents are treated as two&#39;scomplement signed values.</p></summary>
            /// <signature>
            /// <param name="offset">Number</param>
            /// <param name="noAssert">Boolean, Optional, Default: false</param>
            /// <returns>Number</returns>
            /// </signature>
            /// <signature>
            /// <param name="offset"></param>
            /// <param name="noAssert"></param>
            /// </signature>
            /// <signature>
            /// <param name="offset"></param>
            /// <param name="noAssert"></param>
            /// </signature>
        }
        this.readInt16BE = function(offset, noAssert) {
            /// <summary><p>Reads a signed 16 bit integer from the buffer at the specified offset withspecified endian format.</p><p>Set <code>noAssert</code> to true to skip validation of <code>offset</code>. This means that <code>offset</code>may be beyond the end of the buffer. Defaults to <code>false</code>.</p><p>Works as <code>buffer.readUInt16*</code>, except buffer contents are treated as two&#39;scomplement signed values.</p></summary>
            /// <signature>
            /// <param name="offset">Number</param>
            /// <param name="noAssert">Boolean, Optional, Default: false</param>
            /// <returns>Number</returns>
            /// </signature>
            /// <signature>
            /// <param name="offset"></param>
            /// <param name="noAssert"></param>
            /// </signature>
        }
        this.readInt32LE = function(offset, noAssert) {
            /// <summary><p>Reads a signed 32 bit integer from the buffer at the specified offset withspecified endian format.</p><p>Set <code>noAssert</code> to true to skip validation of <code>offset</code>. This means that <code>offset</code>may be beyond the end of the buffer. Defaults to <code>false</code>.</p><p>Works as <code>buffer.readUInt32*</code>, except buffer contents are treated as two&#39;scomplement signed values.</p></summary>
            /// <signature>
            /// <param name="offset">Number</param>
            /// <param name="noAssert">Boolean, Optional, Default: false</param>
            /// <returns>Number</returns>
            /// </signature>
            /// <signature>
            /// <param name="offset"></param>
            /// <param name="noAssert"></param>
            /// </signature>
            /// <signature>
            /// <param name="offset"></param>
            /// <param name="noAssert"></param>
            /// </signature>
        }
        this.readInt32BE = function(offset, noAssert) {
            /// <summary><p>Reads a signed 32 bit integer from the buffer at the specified offset withspecified endian format.</p><p>Set <code>noAssert</code> to true to skip validation of <code>offset</code>. This means that <code>offset</code>may be beyond the end of the buffer. Defaults to <code>false</code>.</p><p>Works as <code>buffer.readUInt32*</code>, except buffer contents are treated as two&#39;scomplement signed values.</p></summary>
            /// <signature>
            /// <param name="offset">Number</param>
            /// <param name="noAssert">Boolean, Optional, Default: false</param>
            /// <returns>Number</returns>
            /// </signature>
            /// <signature>
            /// <param name="offset"></param>
            /// <param name="noAssert"></param>
            /// </signature>
        }
        this.readFloatLE = function(offset, noAssert) {
            /// <summary><p>Reads a 32 bit float from the buffer at the specified offset with specifiedendian format.</p><p>Set <code>noAssert</code> to true to skip validation of <code>offset</code>. This means that <code>offset</code>may be beyond the end of the buffer. Defaults to <code>false</code>.</p><p>Example:</p><pre><code>var buf = new Buffer(4);buf[0] = 0x00;buf[1] = 0x00;buf[2] = 0x80;buf[3] = 0x3f;console.log(buf.readFloatLE(0));// 0x01</code></pre></summary>
            /// <signature>
            /// <param name="offset">Number</param>
            /// <param name="noAssert">Boolean, Optional, Default: false</param>
            /// <returns>Number</returns>
            /// </signature>
            /// <signature>
            /// <param name="offset"></param>
            /// <param name="noAssert"></param>
            /// </signature>
            /// <signature>
            /// <param name="offset"></param>
            /// <param name="noAssert"></param>
            /// </signature>
        }
        this.readFloatBE = function(offset, noAssert) {
            /// <summary><p>Reads a 32 bit float from the buffer at the specified offset with specifiedendian format.</p><p>Set <code>noAssert</code> to true to skip validation of <code>offset</code>. This means that <code>offset</code>may be beyond the end of the buffer. Defaults to <code>false</code>.</p><p>Example:</p><pre><code>var buf = new Buffer(4);buf[0] = 0x00;buf[1] = 0x00;buf[2] = 0x80;buf[3] = 0x3f;console.log(buf.readFloatLE(0));// 0x01</code></pre></summary>
            /// <signature>
            /// <param name="offset">Number</param>
            /// <param name="noAssert">Boolean, Optional, Default: false</param>
            /// <returns>Number</returns>
            /// </signature>
            /// <signature>
            /// <param name="offset"></param>
            /// <param name="noAssert"></param>
            /// </signature>
        }
        this.readDoubleLE = function(offset, noAssert) {
            /// <summary><p>Reads a 64 bit double from the buffer at the specified offset with specifiedendian format.</p><p>Set <code>noAssert</code> to true to skip validation of <code>offset</code>. This means that <code>offset</code>may be beyond the end of the buffer. Defaults to <code>false</code>.</p><p>Example:</p><pre><code>var buf = new Buffer(8);buf[0] = 0x55;buf[1] = 0x55;buf[2] = 0x55;buf[3] = 0x55;buf[4] = 0x55;buf[5] = 0x55;buf[6] = 0xd5;buf[7] = 0x3f;console.log(buf.readDoubleLE(0));// 0.3333333333333333</code></pre></summary>
            /// <signature>
            /// <param name="offset">Number</param>
            /// <param name="noAssert">Boolean, Optional, Default: false</param>
            /// <returns>Number</returns>
            /// </signature>
            /// <signature>
            /// <param name="offset"></param>
            /// <param name="noAssert"></param>
            /// </signature>
            /// <signature>
            /// <param name="offset"></param>
            /// <param name="noAssert"></param>
            /// </signature>
        }
        this.readDoubleBE = function(offset, noAssert) {
            /// <summary><p>Reads a 64 bit double from the buffer at the specified offset with specifiedendian format.</p><p>Set <code>noAssert</code> to true to skip validation of <code>offset</code>. This means that <code>offset</code>may be beyond the end of the buffer. Defaults to <code>false</code>.</p><p>Example:</p><pre><code>var buf = new Buffer(8);buf[0] = 0x55;buf[1] = 0x55;buf[2] = 0x55;buf[3] = 0x55;buf[4] = 0x55;buf[5] = 0x55;buf[6] = 0xd5;buf[7] = 0x3f;console.log(buf.readDoubleLE(0));// 0.3333333333333333</code></pre></summary>
            /// <signature>
            /// <param name="offset">Number</param>
            /// <param name="noAssert">Boolean, Optional, Default: false</param>
            /// <returns>Number</returns>
            /// </signature>
            /// <signature>
            /// <param name="offset"></param>
            /// <param name="noAssert"></param>
            /// </signature>
        }
        this.writeUInt8 = function(value, offset, noAssert) {
            /// <summary><p>Writes <code>value</code> to the buffer at the specified offset. Note, <code>value</code> must be avalid unsigned 8 bit integer.</p><p>Set <code>noAssert</code> to true to skip validation of <code>value</code> and <code>offset</code>. This meansthat <code>value</code> may be too large for the specific function and <code>offset</code> may bebeyond the end of the buffer leading to the values being silently dropped. Thisshould not be used unless you are certain of correctness. Defaults to <code>false</code>.</p><p>Example:</p><pre><code>var buf = new Buffer(4);buf.writeUInt8(0x3, 0);buf.writeUInt8(0x4, 1);buf.writeUInt8(0x23, 2);buf.writeUInt8(0x42, 3);console.log(buf);// &lt;Buffer 03 04 23 42&gt;</code></pre></summary>
            /// <signature>
            /// <param name="value">Number</param>
            /// <param name="offset">Number</param>
            /// <param name="noAssert">Boolean, Optional, Default: false</param>
            /// </signature>
            /// <signature>
            /// <param name="value"></param>
            /// <param name="offset"></param>
            /// <param name="noAssert"></param>
            /// </signature>
        }
        this.writeUInt16LE = function(value, offset, noAssert) {
            /// <summary><p>Writes <code>value</code> to the buffer at the specified offset with specified endianformat. Note, <code>value</code> must be a valid unsigned 16 bit integer.</p><p>Set <code>noAssert</code> to true to skip validation of <code>value</code> and <code>offset</code>. This meansthat <code>value</code> may be too large for the specific function and <code>offset</code> may bebeyond the end of the buffer leading to the values being silently dropped. Thisshould not be used unless you are certain of correctness. Defaults to <code>false</code>.</p><p>Example:</p><pre><code>var buf = new Buffer(4);buf.writeUInt16BE(0xdead, 0);buf.writeUInt16BE(0xbeef, 2);console.log(buf);buf.writeUInt16LE(0xdead, 0);buf.writeUInt16LE(0xbeef, 2);console.log(buf);// &lt;Buffer de ad be ef&gt;// &lt;Buffer ad de ef be&gt;</code></pre></summary>
            /// <signature>
            /// <param name="value">Number</param>
            /// <param name="offset">Number</param>
            /// <param name="noAssert">Boolean, Optional, Default: false</param>
            /// </signature>
            /// <signature>
            /// <param name="value"></param>
            /// <param name="offset"></param>
            /// <param name="noAssert"></param>
            /// </signature>
            /// <signature>
            /// <param name="value"></param>
            /// <param name="offset"></param>
            /// <param name="noAssert"></param>
            /// </signature>
        }
        this.writeUInt16BE = function(value, offset, noAssert) {
            /// <summary><p>Writes <code>value</code> to the buffer at the specified offset with specified endianformat. Note, <code>value</code> must be a valid unsigned 16 bit integer.</p><p>Set <code>noAssert</code> to true to skip validation of <code>value</code> and <code>offset</code>. This meansthat <code>value</code> may be too large for the specific function and <code>offset</code> may bebeyond the end of the buffer leading to the values being silently dropped. Thisshould not be used unless you are certain of correctness. Defaults to <code>false</code>.</p><p>Example:</p><pre><code>var buf = new Buffer(4);buf.writeUInt16BE(0xdead, 0);buf.writeUInt16BE(0xbeef, 2);console.log(buf);buf.writeUInt16LE(0xdead, 0);buf.writeUInt16LE(0xbeef, 2);console.log(buf);// &lt;Buffer de ad be ef&gt;// &lt;Buffer ad de ef be&gt;</code></pre></summary>
            /// <signature>
            /// <param name="value">Number</param>
            /// <param name="offset">Number</param>
            /// <param name="noAssert">Boolean, Optional, Default: false</param>
            /// </signature>
            /// <signature>
            /// <param name="value"></param>
            /// <param name="offset"></param>
            /// <param name="noAssert"></param>
            /// </signature>
        }
        this.writeUInt32LE = function(value, offset, noAssert) {
            /// <summary><p>Writes <code>value</code> to the buffer at the specified offset with specified endianformat. Note, <code>value</code> must be a valid unsigned 32 bit integer.</p><p>Set <code>noAssert</code> to true to skip validation of <code>value</code> and <code>offset</code>. This meansthat <code>value</code> may be too large for the specific function and <code>offset</code> may bebeyond the end of the buffer leading to the values being silently dropped. Thisshould not be used unless you are certain of correctness. Defaults to <code>false</code>.</p><p>Example:</p><pre><code>var buf = new Buffer(4);buf.writeUInt32BE(0xfeedface, 0);console.log(buf);buf.writeUInt32LE(0xfeedface, 0);console.log(buf);// &lt;Buffer fe ed fa ce&gt;// &lt;Buffer ce fa ed fe&gt;</code></pre></summary>
            /// <signature>
            /// <param name="value">Number</param>
            /// <param name="offset">Number</param>
            /// <param name="noAssert">Boolean, Optional, Default: false</param>
            /// </signature>
            /// <signature>
            /// <param name="value"></param>
            /// <param name="offset"></param>
            /// <param name="noAssert"></param>
            /// </signature>
            /// <signature>
            /// <param name="value"></param>
            /// <param name="offset"></param>
            /// <param name="noAssert"></param>
            /// </signature>
        }
        this.writeUInt32BE = function(value, offset, noAssert) {
            /// <summary><p>Writes <code>value</code> to the buffer at the specified offset with specified endianformat. Note, <code>value</code> must be a valid unsigned 32 bit integer.</p><p>Set <code>noAssert</code> to true to skip validation of <code>value</code> and <code>offset</code>. This meansthat <code>value</code> may be too large for the specific function and <code>offset</code> may bebeyond the end of the buffer leading to the values being silently dropped. Thisshould not be used unless you are certain of correctness. Defaults to <code>false</code>.</p><p>Example:</p><pre><code>var buf = new Buffer(4);buf.writeUInt32BE(0xfeedface, 0);console.log(buf);buf.writeUInt32LE(0xfeedface, 0);console.log(buf);// &lt;Buffer fe ed fa ce&gt;// &lt;Buffer ce fa ed fe&gt;</code></pre></summary>
            /// <signature>
            /// <param name="value">Number</param>
            /// <param name="offset">Number</param>
            /// <param name="noAssert">Boolean, Optional, Default: false</param>
            /// </signature>
            /// <signature>
            /// <param name="value"></param>
            /// <param name="offset"></param>
            /// <param name="noAssert"></param>
            /// </signature>
        }
        this.writeInt8 = function(value, offset, noAssert) {
            /// <summary><p>Writes <code>value</code> to the buffer at the specified offset. Note, <code>value</code> must be avalid signed 8 bit integer.</p><p>Set <code>noAssert</code> to true to skip validation of <code>value</code> and <code>offset</code>. This meansthat <code>value</code> may be too large for the specific function and <code>offset</code> may bebeyond the end of the buffer leading to the values being silently dropped. Thisshould not be used unless you are certain of correctness. Defaults to <code>false</code>.</p><p>Works as <code>buffer.writeUInt8</code>, except value is written out as a two&#39;s complementsigned integer into <code>buffer</code>.</p></summary>
            /// <signature>
            /// <param name="value">Number</param>
            /// <param name="offset">Number</param>
            /// <param name="noAssert">Boolean, Optional, Default: false</param>
            /// </signature>
            /// <signature>
            /// <param name="value"></param>
            /// <param name="offset"></param>
            /// <param name="noAssert"></param>
            /// </signature>
        }
        this.writeInt16LE = function(value, offset, noAssert) {
            /// <summary><p>Writes <code>value</code> to the buffer at the specified offset with specified endianformat. Note, <code>value</code> must be a valid signed 16 bit integer.</p><p>Set <code>noAssert</code> to true to skip validation of <code>value</code> and <code>offset</code>. This meansthat <code>value</code> may be too large for the specific function and <code>offset</code> may bebeyond the end of the buffer leading to the values being silently dropped. Thisshould not be used unless you are certain of correctness. Defaults to <code>false</code>.</p><p>Works as <code>buffer.writeUInt16*</code>, except value is written out as a two&#39;scomplement signed integer into <code>buffer</code>.</p></summary>
            /// <signature>
            /// <param name="value">Number</param>
            /// <param name="offset">Number</param>
            /// <param name="noAssert">Boolean, Optional, Default: false</param>
            /// </signature>
            /// <signature>
            /// <param name="value"></param>
            /// <param name="offset"></param>
            /// <param name="noAssert"></param>
            /// </signature>
            /// <signature>
            /// <param name="value"></param>
            /// <param name="offset"></param>
            /// <param name="noAssert"></param>
            /// </signature>
        }
        this.writeInt16BE = function(value, offset, noAssert) {
            /// <summary><p>Writes <code>value</code> to the buffer at the specified offset with specified endianformat. Note, <code>value</code> must be a valid signed 16 bit integer.</p><p>Set <code>noAssert</code> to true to skip validation of <code>value</code> and <code>offset</code>. This meansthat <code>value</code> may be too large for the specific function and <code>offset</code> may bebeyond the end of the buffer leading to the values being silently dropped. Thisshould not be used unless you are certain of correctness. Defaults to <code>false</code>.</p><p>Works as <code>buffer.writeUInt16*</code>, except value is written out as a two&#39;scomplement signed integer into <code>buffer</code>.</p></summary>
            /// <signature>
            /// <param name="value">Number</param>
            /// <param name="offset">Number</param>
            /// <param name="noAssert">Boolean, Optional, Default: false</param>
            /// </signature>
            /// <signature>
            /// <param name="value"></param>
            /// <param name="offset"></param>
            /// <param name="noAssert"></param>
            /// </signature>
        }
        this.writeInt32LE = function(value, offset, noAssert) {
            /// <summary><p>Writes <code>value</code> to the buffer at the specified offset with specified endianformat. Note, <code>value</code> must be a valid signed 32 bit integer.</p><p>Set <code>noAssert</code> to true to skip validation of <code>value</code> and <code>offset</code>. This meansthat <code>value</code> may be too large for the specific function and <code>offset</code> may bebeyond the end of the buffer leading to the values being silently dropped. Thisshould not be used unless you are certain of correctness. Defaults to <code>false</code>.</p><p>Works as <code>buffer.writeUInt32*</code>, except value is written out as a two&#39;scomplement signed integer into <code>buffer</code>.</p></summary>
            /// <signature>
            /// <param name="value">Number</param>
            /// <param name="offset">Number</param>
            /// <param name="noAssert">Boolean, Optional, Default: false</param>
            /// </signature>
            /// <signature>
            /// <param name="value"></param>
            /// <param name="offset"></param>
            /// <param name="noAssert"></param>
            /// </signature>
            /// <signature>
            /// <param name="value"></param>
            /// <param name="offset"></param>
            /// <param name="noAssert"></param>
            /// </signature>
        }
        this.writeInt32BE = function(value, offset, noAssert) {
            /// <summary><p>Writes <code>value</code> to the buffer at the specified offset with specified endianformat. Note, <code>value</code> must be a valid signed 32 bit integer.</p><p>Set <code>noAssert</code> to true to skip validation of <code>value</code> and <code>offset</code>. This meansthat <code>value</code> may be too large for the specific function and <code>offset</code> may bebeyond the end of the buffer leading to the values being silently dropped. Thisshould not be used unless you are certain of correctness. Defaults to <code>false</code>.</p><p>Works as <code>buffer.writeUInt32*</code>, except value is written out as a two&#39;scomplement signed integer into <code>buffer</code>.</p></summary>
            /// <signature>
            /// <param name="value">Number</param>
            /// <param name="offset">Number</param>
            /// <param name="noAssert">Boolean, Optional, Default: false</param>
            /// </signature>
            /// <signature>
            /// <param name="value"></param>
            /// <param name="offset"></param>
            /// <param name="noAssert"></param>
            /// </signature>
        }
        this.writeFloatLE = function(value, offset, noAssert) {
            /// <summary><p>Writes <code>value</code> to the buffer at the specified offset with specified endianformat. Note, <code>value</code> must be a valid 32 bit float.</p><p>Set <code>noAssert</code> to true to skip validation of <code>value</code> and <code>offset</code>. This meansthat <code>value</code> may be too large for the specific function and <code>offset</code> may bebeyond the end of the buffer leading to the values being silently dropped. Thisshould not be used unless you are certain of correctness. Defaults to <code>false</code>.</p><p>Example:</p><pre><code>var buf = new Buffer(4);buf.writeFloatBE(0xcafebabe, 0);console.log(buf);buf.writeFloatLE(0xcafebabe, 0);console.log(buf);// &lt;Buffer 4f 4a fe bb&gt;// &lt;Buffer bb fe 4a 4f&gt;</code></pre></summary>
            /// <signature>
            /// <param name="value">Number</param>
            /// <param name="offset">Number</param>
            /// <param name="noAssert">Boolean, Optional, Default: false</param>
            /// </signature>
            /// <signature>
            /// <param name="value"></param>
            /// <param name="offset"></param>
            /// <param name="noAssert"></param>
            /// </signature>
            /// <signature>
            /// <param name="value"></param>
            /// <param name="offset"></param>
            /// <param name="noAssert"></param>
            /// </signature>
        }
        this.writeFloatBE = function(value, offset, noAssert) {
            /// <summary><p>Writes <code>value</code> to the buffer at the specified offset with specified endianformat. Note, <code>value</code> must be a valid 32 bit float.</p><p>Set <code>noAssert</code> to true to skip validation of <code>value</code> and <code>offset</code>. This meansthat <code>value</code> may be too large for the specific function and <code>offset</code> may bebeyond the end of the buffer leading to the values being silently dropped. Thisshould not be used unless you are certain of correctness. Defaults to <code>false</code>.</p><p>Example:</p><pre><code>var buf = new Buffer(4);buf.writeFloatBE(0xcafebabe, 0);console.log(buf);buf.writeFloatLE(0xcafebabe, 0);console.log(buf);// &lt;Buffer 4f 4a fe bb&gt;// &lt;Buffer bb fe 4a 4f&gt;</code></pre></summary>
            /// <signature>
            /// <param name="value">Number</param>
            /// <param name="offset">Number</param>
            /// <param name="noAssert">Boolean, Optional, Default: false</param>
            /// </signature>
            /// <signature>
            /// <param name="value"></param>
            /// <param name="offset"></param>
            /// <param name="noAssert"></param>
            /// </signature>
        }
        this.writeDoubleLE = function(value, offset, noAssert) {
            /// <summary><p>Writes <code>value</code> to the buffer at the specified offset with specified endianformat. Note, <code>value</code> must be a valid 64 bit double.</p><p>Set <code>noAssert</code> to true to skip validation of <code>value</code> and <code>offset</code>. This meansthat <code>value</code> may be too large for the specific function and <code>offset</code> may bebeyond the end of the buffer leading to the values being silently dropped. Thisshould not be used unless you are certain of correctness. Defaults to <code>false</code>.</p><p>Example:</p><pre><code>var buf = new Buffer(8);buf.writeDoubleBE(0xdeadbeefcafebabe, 0);console.log(buf);buf.writeDoubleLE(0xdeadbeefcafebabe, 0);console.log(buf);// &lt;Buffer 43 eb d5 b7 dd f9 5f d7&gt;// &lt;Buffer d7 5f f9 dd b7 d5 eb 43&gt;</code></pre></summary>
            /// <signature>
            /// <param name="value">Number</param>
            /// <param name="offset">Number</param>
            /// <param name="noAssert">Boolean, Optional, Default: false</param>
            /// </signature>
            /// <signature>
            /// <param name="value"></param>
            /// <param name="offset"></param>
            /// <param name="noAssert"></param>
            /// </signature>
            /// <signature>
            /// <param name="value"></param>
            /// <param name="offset"></param>
            /// <param name="noAssert"></param>
            /// </signature>
        }
        this.writeDoubleBE = function(value, offset, noAssert) {
            /// <summary><p>Writes <code>value</code> to the buffer at the specified offset with specified endianformat. Note, <code>value</code> must be a valid 64 bit double.</p><p>Set <code>noAssert</code> to true to skip validation of <code>value</code> and <code>offset</code>. This meansthat <code>value</code> may be too large for the specific function and <code>offset</code> may bebeyond the end of the buffer leading to the values being silently dropped. Thisshould not be used unless you are certain of correctness. Defaults to <code>false</code>.</p><p>Example:</p><pre><code>var buf = new Buffer(8);buf.writeDoubleBE(0xdeadbeefcafebabe, 0);console.log(buf);buf.writeDoubleLE(0xdeadbeefcafebabe, 0);console.log(buf);// &lt;Buffer 43 eb d5 b7 dd f9 5f d7&gt;// &lt;Buffer d7 5f f9 dd b7 d5 eb 43&gt;</code></pre></summary>
            /// <signature>
            /// <param name="value">Number</param>
            /// <param name="offset">Number</param>
            /// <param name="noAssert">Boolean, Optional, Default: false</param>
            /// </signature>
            /// <signature>
            /// <param name="value"></param>
            /// <param name="offset"></param>
            /// <param name="noAssert"></param>
            /// </signature>
        }
        this.fill = function(value, offset, end) {
            /// <summary><p>Fills the buffer with the specified value. If the <code>offset</code> (defaults to <code>0</code>)and <code>end</code> (defaults to <code>buffer.length</code>) are not given it will fill the entirebuffer.</p><pre><code>var b = new Buffer(50);b.fill(&quot;h&quot;);</code></pre></summary>
            /// <signature>
            /// <param name="value"></param>
            /// <param name="offset">Number</param>
            /// <param name="end">Number</param>
            /// </signature>
            /// <signature>
            /// <param name="value"></param>
            /// <param name="offset"></param>
            /// <param name="end"></param>
            /// </signature>
        }
        /// <field name='[index]'><p>Get and set the octet at <code>index</code>. The values refer to individual bytes,so the legal range is between <code>0x00</code> and <code>0xFF</code> hex or <code>0</code> and <code>255</code>.</p><p>Example: copy an ASCII string into a buffer, one byte at a time:</p><pre><code>str = &quot;node.js&quot;;buf = new Buffer(str.length);for (var i = 0; i &lt; str.length ; i++) {  buf[i] = str.charCodeAt(i);}console.log(buf);// node.js</code></pre></field>
        this.[index] = undefined;
        /// <field name='length'><p>The size of the buffer in bytes.  Note that this is not necessarily the sizeof the contents. <code>length</code> refers to the amount of memory allocated for thebuffer object.  It does not change when the contents of the buffer are changed.</p><pre><code>buf = new Buffer(1234);console.log(buf.length);buf.write(&quot;some string&quot;, 0, &quot;ascii&quot;);console.log(buf.length);// 1234// 1234</code></pre></field>
        this.length = undefined;
    }
    this.SlowBuffer = function() {
    }
    /// <field name='INSPECT_MAX_BYTES'><p>How many bytes will be returned when <code>buffer.inspect()</code> is called. This canbe overridden by user modules.</p><p>Note that this is a property on the buffer module returned by<code>require(&#39;buffer&#39;)</code>, not on the Buffer global, or a buffer instance.</p></field>
    this.INSPECT_MAX_BYTES = undefined;
} // module buffer

function stream() {
    /// <summary><p>A stream is an abstract interface implemented by various objects inNode.  For example a request to an HTTP server is a stream, as isstdout. Streams are readable, writable, or both. All streams areinstances of [EventEmitter][]</p><p>You can load up the Stream base class by doing <code>require(&#39;stream&#39;)</code>.</p></summary>
    this.ReadableStream = function() {
        this.setEncoding = function(encoding) {
            /// <summary><p>Makes the <code>&#39;data&#39;</code> event emit a string instead of a <code>Buffer</code>. <code>encoding</code>can be <code>&#39;utf8&#39;</code>, <code>&#39;utf16le&#39;</code> (<code>&#39;ucs2&#39;</code>), <code>&#39;ascii&#39;</code>, or <code>&#39;hex&#39;</code>. Defaultsto <code>&#39;utf8&#39;</code>.</p></summary>
            /// <signature>
            /// <param name="encoding"></param>
            /// </signature>
        }
        this.pause = function() {
            /// <summary><p>Issues an advisory signal to the underlying communication layer,requesting that no further data be sent until <code>resume()</code> is called.</p><p>Note that, due to the advisory nature, certain streams will not bepaused immediately, and so <code>&#39;data&#39;</code> events may be emitted for someindeterminate period of time even after <code>pause()</code> is called. You maywish to buffer such <code>&#39;data&#39;</code> events.</p></summary>
        }
        this.resume = function() {
            /// <summary><p>Resumes the incoming <code>&#39;data&#39;</code> events after a <code>pause()</code>.</p></summary>
        }
        this.destroy = function() {
            /// <summary><p>Closes the underlying file descriptor. Stream is no longer <code>writable</code>nor <code>readable</code>.  The stream will not emit any more &#39;data&#39;, or &#39;end&#39;events. Any queued write data will not be sent.  The stream should emit&#39;close&#39; event once its resources have been disposed of.</p></summary>
        }
        this.pipe = function(destination, options) {
            /// <summary><p>This is a <code>Stream.prototype</code> method available on all <code>Stream</code>s.</p><p>Connects this read stream to <code>destination</code> WriteStream. Incoming data onthis stream gets written to <code>destination</code>. The destination and sourcestreams are kept in sync by pausing and resuming as necessary.</p><p>This function returns the <code>destination</code> stream.</p><p>Emulating the Unix <code>cat</code> command:</p><pre><code>process.stdin.resume(); process.stdin.pipe(process.stdout);</code></pre><p>By default <code>end()</code> is called on the destination when the source streamemits <code>end</code>, so that <code>destination</code> is no longer writable. Pass <code>{ end:false }</code> as <code>options</code> to keep the destination stream open.</p><p>This keeps <code>process.stdout</code> open so that &quot;Goodbye&quot; can be written at theend.</p><pre><code>process.stdin.resume();process.stdin.pipe(process.stdout, { end: false });process.stdin.on(&quot;end&quot;, function() {process.stdout.write(&quot;Goodbye\n&quot;); });</code></pre></summary>
            /// <signature>
            /// <param name="destination"></param>
            /// <param name="options"></param>
            /// </signature>
        }
        emitter = new Events().EventEmitter;
        /// <field name='data'><p><code>function (data) { }</code></p><p>The <code>&#39;data&#39;</code> event emits either a <code>Buffer</code> (by default) or a string if<code>setEncoding()</code> was used.</p><p>Note that the <strong>data will be lost</strong> if there is no listener when a<code>Readable Stream</code> emits a <code>&#39;data&#39;</code> event.</p></field>
        this.data = new emitter();
        /// <field name='end'><p><code>function () { }</code></p><p>Emitted when the stream has received an EOF (FIN in TCP terminology).Indicates that no more <code>&#39;data&#39;</code> events will happen. If the stream isalso writable, it may be possible to continue writing.</p></field>
        this.end = new emitter();
        /// <field name='error'><p><code>function (exception) { }</code></p><p>Emitted if there was an error receiving data.</p></field>
        this.error = new emitter();
        /// <field name='close'><p><code>function () { }</code></p><p>Emitted when the underlying resource (for example, the backing filedescriptor) has been closed. Not all streams will emit this.</p></field>
        this.close = new emitter();
        /// <field name='readable'><p>A boolean that is <code>true</code> by default, but turns <code>false</code> after an<code>&#39;error&#39;</code> occurred, the stream came to an <code>&#39;end&#39;</code>, or <code>destroy()</code> wascalled.</p></field>
        this.readable = undefined;
    }
    this.WritableStream = function() {
        this.write = function(string, encoding) {
            /// <summary><p>Writes <code>string</code> with the given <code>encoding</code> to the stream.  Returns <code>true</code>if the string has been flushed to the kernel buffer.  Returns <code>false</code> toindicate that the kernel buffer is full, and the data will be sent outin the future. The <code>&#39;drain&#39;</code> event will indicate when the kernel bufferis empty again. The <code>encoding</code> defaults to <code>&#39;utf8&#39;</code>.</p></summary>
            /// <signature>
            /// <param name="string"></param>
            /// <param name="encoding"></param>
            /// </signature>
        }
        this.write = function(buffer) {
            /// <summary><p>Same as the above except with a raw buffer.</p></summary>
            /// <signature>
            /// <param name="buffer"></param>
            /// </signature>
        }
        this.end = function() {
            /// <summary><p>Terminates the stream with EOF or FIN.  This call will allow queuedwrite data to be sent before closing the stream.</p></summary>
        }
        this.end = function(string, encoding) {
            /// <summary><p>Sends <code>string</code> with the given <code>encoding</code> and terminates the stream withEOF or FIN. This is useful to reduce the number of packets sent.</p></summary>
            /// <signature>
            /// <param name="string"></param>
            /// <param name="encoding"></param>
            /// </signature>
        }
        this.end = function(buffer) {
            /// <summary><p>Same as above but with a <code>buffer</code>.</p></summary>
            /// <signature>
            /// <param name="buffer"></param>
            /// </signature>
        }
        this.destroy = function() {
            /// <summary><p>Closes the underlying file descriptor. Stream is no longer <code>writable</code>nor <code>readable</code>.  The stream will not emit any more &#39;data&#39;, or &#39;end&#39;events. Any queued write data will not be sent.  The stream should emit&#39;close&#39; event once its resources have been disposed of.</p></summary>
        }
        this.destroySoon = function() {
            /// <summary><p>After the write queue is drained, close the file descriptor.<code>destroySoon()</code> can still destroy straight away, as long as there is nodata left in the queue for writes.</p></summary>
        }
        emitter = new Events().EventEmitter;
        /// <field name='drain'><p><code>function () { }</code></p><p>Emitted when the stream&#39;s write queue empties and it&#39;s safe to write withoutbuffering again. Listen for it when <code>stream.write()</code> returns <code>false</code>.</p><p>The <code>&#39;drain&#39;</code> event can happen at <em>any</em> time, regardless of whether or not<code>stream.write()</code> has previously returned <code>false</code>. To avoid receiving unwanted<code>&#39;drain&#39;</code> events, listen using <code>stream.once()</code>.</p></field>
        this.drain = new emitter();
        /// <field name='error'><p><code>function (exception) { }</code></p><p>Emitted on error with the exception <code>exception</code>.</p></field>
        this.error = new emitter();
        /// <field name='close'><p><code>function () { }</code></p><p>Emitted when the underlying file descriptor has been closed.</p></field>
        this.close = new emitter();
        /// <field name='pipe'><p><code>function (src) { }</code></p><p>Emitted when the stream is passed to a readable stream&#39;s pipe method.</p></field>
        this.pipe = new emitter();
        /// <field name='writable'><p>A boolean that is <code>true</code> by default, but turns <code>false</code> after an<code>&#39;error&#39;</code> occurred or <code>end()</code> / <code>destroy()</code> was called.</p></field>
        this.writable = undefined;
    }
} // module stream

function crypto() {
    /// <summary><pre><code>Stability: 2 - Unstable; API changes are being discussed forfuture versions.  Breaking changes will be minimized.  See below.</code></pre><p>Use <code>require(&#39;crypto&#39;)</code> to access this module.</p><p>The crypto module requires OpenSSL to be available on the underlying platform.It offers a way of encapsulating secure credentials to be used as partof a secure HTTPS net or http connection.</p><p>It also offers a set of wrappers for OpenSSL&#39;s hash, hmac, cipher, decipher, sign and verify methods.</p></summary>
    this.createCredentials = function(details) {
        /// <summary><p>Creates a credentials object, with the optional details being a dictionary with keys:</p><ul><li><code>pfx</code> : A string or buffer holding the PFX or PKCS12 encoded private key, certificate and CA certificates</li><li><code>key</code> : A string holding the PEM encoded private key</li><li><code>passphrase</code> : A string of passphrase for the private key or pfx</li><li><code>cert</code> : A string holding the PEM encoded certificate</li><li><code>ca</code> : Either a string or list of strings of PEM encoded CA certificates to trust.</li><li><code>crl</code> : Either a string or list of strings of PEM encoded CRLs (Certificate Revocation List)</li><li><code>ciphers</code>: A string describing the ciphers to use or exclude. Consult<a href="http://www.openssl.org/docs/apps/ciphers.html#CIPHER_LIST_FORMAT">http://www.openssl.org/docs/apps/ciphers.html#CIPHER_LIST_FORMAT</a> for detailson the format.</li></ul><p>If no &#39;ca&#39; details are given, then node.js will use the default publicly trusted list of CAs as given in</p><p><a href="http://mxr.mozilla.org/mozilla/source/security/nss/lib/ckfw/builtins/certdata.txt">http://mxr.mozilla.org/mozilla/source/security/nss/lib/ckfw/builtins/certdata.txt</a>.</p></summary>
        /// <signature>
        /// <param name="details"></param>
        /// </signature>
        return new crypto.Credentials();
    }
    this.createHash = function(algorithm) {
        /// <summary><p>Creates and returns a hash object, a cryptographic hash with the given algorithmwhich can be used to generate hash digests.</p><p><code>algorithm</code> is dependent on the available algorithms supported by the versionof OpenSSL on the platform. Examples are <code>&#39;sha1&#39;</code>, <code>&#39;md5&#39;</code>, <code>&#39;sha256&#39;</code>, <code>&#39;sha512&#39;</code>, etc.On recent releases, <code>openssl list-message-digest-algorithms</code> will display the available digest algorithms.</p><p>Example: this program that takes the sha1 sum of a file</p><pre><code>var filename = process.argv[2];var crypto = require(&#39;crypto&#39;);var fs = require(&#39;fs&#39;);var shasum = crypto.createHash(&#39;sha1&#39;);var s = fs.ReadStream(filename);s.on(&#39;data&#39;, function(d) {  shasum.update(d);});s.on(&#39;end&#39;, function() {  var d = shasum.digest(&#39;hex&#39;);  console.log(d + &#39;  &#39; + filename);});</code></pre></summary>
        /// <signature>
        /// <param name="algorithm"></param>
        /// </signature>
        return new crypto.Hash();
    }
    this.createHmac = function(algorithm, key) {
        /// <summary><p>Creates and returns a hmac object, a cryptographic hmac with the given algorithm and key.</p><p><code>algorithm</code> is dependent on the available algorithms supported by OpenSSL - see createHash above.<code>key</code> is the hmac key to be used.</p></summary>
        /// <signature>
        /// <param name="algorithm"></param>
        /// <param name="key"></param>
        /// </signature>
        return new crypto.Hmac();
    }
    this.createCipher = function(algorithm, password) {
        /// <summary><p>Creates and returns a cipher object, with the given algorithm and password.</p><p><code>algorithm</code> is dependent on OpenSSL, examples are <code>&#39;aes192&#39;</code>, etc.On recent releases, <code>openssl list-cipher-algorithms</code> will display theavailable cipher algorithms.<code>password</code> is used to derive key and IV, which must be a <code>&#39;binary&#39;</code> encodedstring or a <a href="buffer.html">buffer</a>.</p></summary>
        /// <signature>
        /// <param name="algorithm"></param>
        /// <param name="password"></param>
        /// </signature>
        return new crypto.Cipher();
    }
    this.createCipheriv = function(algorithm, key, iv) {
        /// <summary><p>Creates and returns a cipher object, with the given algorithm, key and iv.</p><p><code>algorithm</code> is the same as the argument to <code>createCipher()</code>.<code>key</code> is the raw key used by the algorithm.<code>iv</code> is an <a href="http://en.wikipedia.org/wiki/Initialization_vector">initializationvector</a>.</p><p><code>key</code> and <code>iv</code> must be <code>&#39;binary&#39;</code> encoded strings or <a href="buffer.html">buffers</a>.</p></summary>
        /// <signature>
        /// <param name="algorithm"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// </signature>
        return new crypto.Cipheriv();
    }
    this.createDecipher = function(algorithm, password) {
        /// <summary><p>Creates and returns a decipher object, with the given algorithm and key.This is the mirror of the [createCipher()][] above.</p></summary>
        /// <signature>
        /// <param name="algorithm"></param>
        /// <param name="password"></param>
        /// </signature>
        return new crypto.Decipher();
    }
    this.createDecipheriv = function(algorithm, key, iv) {
        /// <summary><p>Creates and returns a decipher object, with the given algorithm, key and iv.This is the mirror of the [createCipheriv()][] above.</p></summary>
        /// <signature>
        /// <param name="algorithm"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// </signature>
        return new crypto.Decipheriv();
    }
    this.createSign = function(algorithm) {
        /// <summary><p>Creates and returns a signing object, with the given algorithm.On recent OpenSSL releases, <code>openssl list-public-key-algorithms</code> will displaythe available signing algorithms. Examples are <code>&#39;RSA-SHA256&#39;</code>.</p></summary>
        /// <signature>
        /// <param name="algorithm"></param>
        /// </signature>
        return new crypto.Sign();
    }
    this.createVerify = function(algorithm) {
        /// <summary><p>Creates and returns a verification object, with the given algorithm.This is the mirror of the signing object above.</p></summary>
        /// <signature>
        /// <param name="algorithm"></param>
        /// </signature>
        return new crypto.Verify();
    }
    this.createDiffieHellman = function(prime_length) {
        /// <summary><p>Creates a Diffie-Hellman key exchange object and generates a prime of thegiven bit length. The generator used is <code>2</code>.</p></summary>
        /// <signature>
        /// <param name="prime_length"></param>
        /// </signature>
        return new crypto.DiffieHellman();
    }
    this.createDiffieHellman = function(prime, encoding) {
        /// <summary><p>Creates a Diffie-Hellman key exchange object using the supplied prime. Thegenerator used is <code>2</code>. Encoding can be <code>&#39;binary&#39;</code>, <code>&#39;hex&#39;</code>, or <code>&#39;base64&#39;</code>.Defaults to <code>&#39;binary&#39;</code>.</p></summary>
        /// <signature>
        /// <param name="prime"></param>
        /// <param name="encoding"></param>
        /// </signature>
        return new crypto.DiffieHellman();
    }
    this.getDiffieHellman = function(group_name) {
        /// <summary><p>Creates a predefined Diffie-Hellman key exchange object.The supported groups are: <code>&#39;modp1&#39;</code>, <code>&#39;modp2&#39;</code>, <code>&#39;modp5&#39;</code>(defined in [RFC 2412][])and <code>&#39;modp14&#39;</code>, <code>&#39;modp15&#39;</code>, <code>&#39;modp16&#39;</code>, <code>&#39;modp17&#39;</code>, <code>&#39;modp18&#39;</code>(defined in [RFC 3526][]).The returned object mimics the interface of objects created by[crypto.createDiffieHellman()][] above, butwill not allow to change the keys (with[diffieHellman.setPublicKey()][] for example).The advantage of using this routine is that the parties don&#39;t have togenerate nor exchange group modulus beforehand, saving both processor andcommunication time.</p><p>Example (obtaining a shared secret):</p><pre><code>var crypto = require(&#39;crypto&#39;);var alice = crypto.getDiffieHellman(&#39;modp5&#39;);var bob = crypto.getDiffieHellman(&#39;modp5&#39;);alice.generateKeys();bob.generateKeys();var alice_secret = alice.computeSecret(bob.getPublicKey(), &#39;binary&#39;, &#39;hex&#39;);var bob_secret = bob.computeSecret(alice.getPublicKey(), &#39;binary&#39;, &#39;hex&#39;);/* alice_secret and bob_secret should be the same */console.log(alice_secret == bob_secret);</code></pre></summary>
        /// <signature>
        /// <param name="group_name"></param>
        /// </signature>
    }
    this.pbkdf2 = function(password, salt, iterations, keylen, callback) {
        /// <summary><p>Asynchronous PBKDF2 applies pseudorandom function HMAC-SHA1 to derivea key of given length from the given password, salt and iterations.The callback gets two arguments <code>(err, derivedKey)</code>.</p></summary>
        /// <signature>
        /// <param name="password"></param>
        /// <param name="salt"></param>
        /// <param name="iterations"></param>
        /// <param name="keylen"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.randomBytes = function(size, callback) {
        /// <summary><p>Generates cryptographically strong pseudo-random data. Usage:</p><pre><code>// asynccrypto.randomBytes(256, function(ex, buf) {  if (ex) throw ex;  console.log(&#39;Have %d bytes of random data: %s&#39;, buf.length, buf);});// synctry {  var buf = crypto.randomBytes(256);  console.log(&#39;Have %d bytes of random data: %s&#39;, buf.length, buf);} catch (ex) {  // handle error}</code></pre></summary>
        /// <signature>
        /// <param name="size"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.Hash = function() {
        this.update = function(data, input_encoding) {
            /// <summary><p>Updates the hash content with the given <code>data</code>, the encoding of which is givenin <code>input_encoding</code> and can be <code>&#39;utf8&#39;</code>, <code>&#39;ascii&#39;</code> or <code>&#39;binary&#39;</code>.Defaults to <code>&#39;binary&#39;</code>.This can be called many times with new data as it is streamed.</p></summary>
            /// <signature>
            /// <param name="data"></param>
            /// <param name="input_encoding"></param>
            /// </signature>
        }
        this.digest = function(encoding) {
            /// <summary><p>Calculates the digest of all of the passed data to be hashed.The <code>encoding</code> can be <code>&#39;hex&#39;</code>, <code>&#39;binary&#39;</code> or <code>&#39;base64&#39;</code>.Defaults to <code>&#39;binary&#39;</code>.</p><p>Note: <code>hash</code> object can not be used after <code>digest()</code> method been called.</p></summary>
            /// <signature>
            /// <param name="encoding"></param>
            /// </signature>
        }
    }
    this.Hmac = function() {
        this.update = function(data) {
            /// <summary><p>Update the hmac content with the given <code>data</code>.This can be called many times with new data as it is streamed.</p></summary>
            /// <signature>
            /// <param name="data"></param>
            /// </signature>
        }
        this.digest = function(encoding) {
            /// <summary><p>Calculates the digest of all of the passed data to the hmac.The <code>encoding</code> can be <code>&#39;hex&#39;</code>, <code>&#39;binary&#39;</code> or <code>&#39;base64&#39;</code>.Defaults to <code>&#39;binary&#39;</code>.</p><p>Note: <code>hmac</code> object can not be used after <code>digest()</code> method been called.</p></summary>
            /// <signature>
            /// <param name="encoding"></param>
            /// </signature>
        }
    }
    this.Cipher = function() {
        this.update = function(data, input_encoding, output_encoding) {
            /// <summary><p>Updates the cipher with <code>data</code>, the encoding of which is given in<code>input_encoding</code> and can be <code>&#39;utf8&#39;</code>, <code>&#39;ascii&#39;</code> or <code>&#39;binary&#39;</code>.Defaults to <code>&#39;binary&#39;</code>.</p><p>The <code>output_encoding</code> specifies the output format of the enciphered data,and can be <code>&#39;binary&#39;</code>, <code>&#39;base64&#39;</code> or <code>&#39;hex&#39;</code>. Defaults to <code>&#39;binary&#39;</code>.</p><p>Returns the enciphered contents, and can be called many times with new data as it is streamed.</p></summary>
            /// <signature>
            /// <param name="data"></param>
            /// <param name="input_encoding"></param>
            /// <param name="output_encoding"></param>
            /// </signature>
        }
        this.final = function(output_encoding) {
            /// <summary><p>Returns any remaining enciphered contents, with <code>output_encoding</code> being one of:<code>&#39;binary&#39;</code>, <code>&#39;base64&#39;</code> or <code>&#39;hex&#39;</code>. Defaults to <code>&#39;binary&#39;</code>.</p><p>Note: <code>cipher</code> object can not be used after <code>final()</code> method been called.</p></summary>
            /// <signature>
            /// <param name="output_encoding"></param>
            /// </signature>
        }
        this.setAutoPadding = function(auto_padding) {
            /// <summary><p>You can disable automatic padding of the input data to block size. If <code>auto_padding</code> is false,the length of the entire input data must be a multiple of the cipher&#39;s block size or <code>final</code> will fail.Useful for non-standard padding, e.g. using <code>0x0</code> instead of PKCS padding. You must call this before <code>cipher.final</code>.</p></summary>
            /// <signature>
            /// <param name="auto_padding"></param>
            /// </signature>
        }
    }
    this.Decipher = function() {
        this.update = function(data, input_encoding, output_encoding) {
            /// <summary><p>Updates the decipher with <code>data</code>, which is encoded in <code>&#39;binary&#39;</code>, <code>&#39;base64&#39;</code>or <code>&#39;hex&#39;</code>. Defaults to <code>&#39;binary&#39;</code>.</p><p>The <code>output_decoding</code> specifies in what format to return the decipheredplaintext: <code>&#39;binary&#39;</code>, <code>&#39;ascii&#39;</code> or <code>&#39;utf8&#39;</code>. Defaults to <code>&#39;binary&#39;</code>.</p></summary>
            /// <signature>
            /// <param name="data"></param>
            /// <param name="input_encoding"></param>
            /// <param name="output_encoding"></param>
            /// </signature>
        }
        this.final = function(output_encoding) {
            /// <summary><p>Returns any remaining plaintext which is deciphered,with <code>output_encoding</code> being one of: <code>&#39;binary&#39;</code>, <code>&#39;ascii&#39;</code> or <code>&#39;utf8&#39;</code>.Defaults to <code>&#39;binary&#39;</code>.</p><p>Note: <code>decipher</code> object can not be used after <code>final()</code> method been called.</p></summary>
            /// <signature>
            /// <param name="output_encoding"></param>
            /// </signature>
        }
        this.setAutoPadding = function(auto_padding) {
            /// <summary><p>You can disable auto padding if the data has been encrypted without standard block padding to prevent<code>decipher.final</code> from checking and removing it. Can only work if the input data&#39;s length is a multiple of theciphers block size. You must call this before streaming data to <code>decipher.update</code>.</p></summary>
            /// <signature>
            /// <param name="auto_padding"></param>
            /// </signature>
        }
    }
    this.Signer = function() {
        this.update = function(data) {
            /// <summary><p>Updates the signer object with data.This can be called many times with new data as it is streamed.</p></summary>
            /// <signature>
            /// <param name="data"></param>
            /// </signature>
        }
        this.sign = function(private_key, output_format) {
            /// <summary><p>Calculates the signature on all the updated data passed through the signer.<code>private_key</code> is a string containing the PEM encoded private key for signing.</p><p>Returns the signature in <code>output_format</code> which can be <code>&#39;binary&#39;</code>, <code>&#39;hex&#39;</code> or<code>&#39;base64&#39;</code>. Defaults to <code>&#39;binary&#39;</code>.</p><p>Note: <code>signer</code> object can not be used after <code>sign()</code> method been called.</p></summary>
            /// <signature>
            /// <param name="private_key"></param>
            /// <param name="output_format"></param>
            /// </signature>
        }
    }
    this.Verify = function() {
        this.update = function(data) {
            /// <summary><p>Updates the verifier object with data.This can be called many times with new data as it is streamed.</p></summary>
            /// <signature>
            /// <param name="data"></param>
            /// </signature>
        }
        this.verify = function(object, signature, signature_format) {
            /// <summary><p>Verifies the signed data by using the <code>object</code> and <code>signature</code>. <code>object</code> is  astring containing a PEM encoded object, which can be one of RSA public key,DSA public key, or X.509 certificate. <code>signature</code> is the previously calculatedsignature for the data, in the <code>signature_format</code> which can be <code>&#39;binary&#39;</code>,<code>&#39;hex&#39;</code> or <code>&#39;base64&#39;</code>. Defaults to <code>&#39;binary&#39;</code>.</p><p>Returns true or false depending on the validity of the signature for the data and public key.</p><p>Note: <code>verifier</code> object can not be used after <code>verify()</code> method been called.</p></summary>
            /// <signature>
            /// <param name="object"></param>
            /// <param name="signature"></param>
            /// <param name="signature_format"></param>
            /// </signature>
        }
    }
    this.DiffieHellman = function() {
        this.generateKeys = function(encoding) {
            /// <summary><p>Generates private and public Diffie-Hellman key values, and returns thepublic key in the specified encoding. This key should be transferred to theother party. Encoding can be <code>&#39;binary&#39;</code>, <code>&#39;hex&#39;</code>, or <code>&#39;base64&#39;</code>.Defaults to <code>&#39;binary&#39;</code>.</p></summary>
            /// <signature>
            /// <param name="encoding"></param>
            /// </signature>
        }
        this.computeSecret = function(other_public_key, input_encoding, output_encoding) {
            /// <summary><p>Computes the shared secret using <code>other_public_key</code> as the other party&#39;spublic key and returns the computed shared secret. Supplied key isinterpreted using specified <code>input_encoding</code>, and secret is encoded usingspecified <code>output_encoding</code>. Encodings can be <code>&#39;binary&#39;</code>, <code>&#39;hex&#39;</code>, or<code>&#39;base64&#39;</code>. The input encoding defaults to <code>&#39;binary&#39;</code>.If no output encoding is given, the input encoding is used as output encoding.</p></summary>
            /// <signature>
            /// <param name="other_public_key"></param>
            /// <param name="input_encoding"></param>
            /// <param name="output_encoding"></param>
            /// </signature>
        }
        this.getPrime = function(encoding) {
            /// <summary><p>Returns the Diffie-Hellman prime in the specified encoding, which can be<code>&#39;binary&#39;</code>, <code>&#39;hex&#39;</code>, or <code>&#39;base64&#39;</code>. Defaults to <code>&#39;binary&#39;</code>.</p></summary>
            /// <signature>
            /// <param name="encoding"></param>
            /// </signature>
        }
        this.getGenerator = function(encoding) {
            /// <summary><p>Returns the Diffie-Hellman prime in the specified encoding, which can be<code>&#39;binary&#39;</code>, <code>&#39;hex&#39;</code>, or <code>&#39;base64&#39;</code>. Defaults to <code>&#39;binary&#39;</code>.</p></summary>
            /// <signature>
            /// <param name="encoding"></param>
            /// </signature>
        }
        this.getPublicKey = function(encoding) {
            /// <summary><p>Returns the Diffie-Hellman public key in the specified encoding, which canbe <code>&#39;binary&#39;</code>, <code>&#39;hex&#39;</code>, or <code>&#39;base64&#39;</code>. Defaults to <code>&#39;binary&#39;</code>.</p></summary>
            /// <signature>
            /// <param name="encoding"></param>
            /// </signature>
        }
        this.getPrivateKey = function(encoding) {
            /// <summary><p>Returns the Diffie-Hellman private key in the specified encoding, which canbe <code>&#39;binary&#39;</code>, <code>&#39;hex&#39;</code>, or <code>&#39;base64&#39;</code>. Defaults to <code>&#39;binary&#39;</code>.</p></summary>
            /// <signature>
            /// <param name="encoding"></param>
            /// </signature>
        }
        this.setPublicKey = function(public_key, encoding) {
            /// <summary><p>Sets the Diffie-Hellman public key. Key encoding can be <code>&#39;binary&#39;</code>, <code>&#39;hex&#39;</code>,or <code>&#39;base64&#39;</code>. Defaults to <code>&#39;binary&#39;</code>.</p></summary>
            /// <signature>
            /// <param name="public_key"></param>
            /// <param name="encoding"></param>
            /// </signature>
        }
        this.setPrivateKey = function(public_key, encoding) {
            /// <summary><p>Sets the Diffie-Hellman private key. Key encoding can be <code>&#39;binary&#39;</code>, <code>&#39;hex&#39;</code>,or <code>&#39;base64&#39;</code>. Defaults to <code>&#39;binary&#39;</code>.</p></summary>
            /// <signature>
            /// <param name="public_key"></param>
            /// <param name="encoding"></param>
            /// </signature>
        }
    }
} // module crypto

function tls_() {
    /// <summary><p>Use <code>require(&#39;tls&#39;)</code> to access this module.</p><p>The <code>tls</code> module uses OpenSSL to provide Transport Layer Security and/orSecure Socket Layer: encrypted stream communication.</p><p>TLS/SSL is a public/private key infrastructure. Each client and eachserver must have a private key. A private key is created like this</p><pre><code>openssl genrsa -out ryans-key.pem 1024</code></pre><p>All severs and some clients need to have a certificate. Certificates are publickeys signed by a Certificate Authority or self-signed. The first step togetting a certificate is to create a &quot;Certificate Signing Request&quot; (CSR)file. This is done with:</p><pre><code>openssl req -new -key ryans-key.pem -out ryans-csr.pem</code></pre><p>To create a self-signed certificate with the CSR, do this:</p><pre><code>openssl x509 -req -in ryans-csr.pem -signkey ryans-key.pem -out ryans-cert.pem</code></pre><p>Alternatively you can send the CSR to a Certificate Authority for signing.</p><p>(TODO: docs on creating a CA, for now interested users should just look at<code>test/fixtures/keys/Makefile</code> in the Node source code)</p><p>To create .pfx or .p12, do this:</p><pre><code>openssl pkcs12 -export -in agent5-cert.pem -inkey agent5-key.pem \    -certfile ca-cert.pem -out agent5.pfx</code></pre><ul><li><code>in</code>:  certificate</li><li><code>inkey</code>: private key</li><li><code>certfile</code>: all CA certs concatenated in one file like<code>cat ca1-cert.pem ca2-cert.pem &gt; ca-cert.pem</code></li></ul></summary>
    this.createServer = function(options, secureConnectionListener) {
        /// <summary><p>Creates a new [tls.Server][].  The <code>connectionListener</code> argument isautomatically set as a listener for the [secureConnection][] event.  The<code>options</code> object has these possibilities:</p><ul><li><p><code>pfx</code>: A string or <code>Buffer</code> containing the private key, certificate andCA certs of the server in PFX or PKCS12 format. (Mutually exclusive withthe <code>key</code>, <code>cert</code> and <code>ca</code> options.)</p></li><li><p><code>key</code>: A string or <code>Buffer</code> containing the private key of the server inPEM format. (Required)</p></li><li><p><code>passphrase</code>: A string of passphrase for the private key or pfx.</p></li><li><p><code>cert</code>: A string or <code>Buffer</code> containing the certificate key of the server inPEM format. (Required)</p></li><li><p><code>ca</code>: An array of strings or <code>Buffer</code>s of trusted certificates. If this isomitted several well known &quot;root&quot; CAs will be used, like VeriSign.These are used to authorize connections.</p></li><li><p><code>crl</code> : Either a string or list of strings of PEM encoded CRLs (CertificateRevocation List)</p></li><li><p><code>ciphers</code>: A string describing the ciphers to use or exclude.</p><p>To mitigate [BEAST attacks] it is recommended that you use this option inconjunction with the <code>honorCipherOrder</code> option described below toprioritize the non-CBC cipher.</p><p>Defaults to<code>ECDHE-RSA-AES128-SHA256:AES128-GCM-SHA256:RC4:HIGH:!MD5:!aNULL:!EDH</code>.Consult the [OpenSSL cipher list format documentation] for details on theformat.</p><p><code>ECDHE-RSA-AES128-SHA256</code> and <code>AES128-GCM-SHA256</code> are used when node.js islinked against OpenSSL 1.0.1 or newer and the client speaks TLS 1.2, RC4 isused as a secure fallback.</p><p><strong>NOTE</strong>: Previous revisions of this section suggested <code>AES256-SHA</code> as anacceptable cipher. Unfortunately, <code>AES256-SHA</code> is a CBC cipher and thereforesusceptible to BEAST attacks. Do <em>not</em> use it.</p></li><li><p><code>honorCipherOrder</code> : When choosing a cipher, use the server&#39;s preferencesinstead of the client preferences.</p><p>Note that if SSLv2 is used, the server will send its list of preferencesto the client, and the client chooses the cipher.</p><p>Although, this option is disabled by default, it is <em>recommended</em> that youuse this option in conjunction with the <code>ciphers</code> option to mitigateBEAST attacks.</p></li><li><p><code>requestCert</code>: If <code>true</code> the server will request a certificate fromclients that connect and attempt to verify that certificate. Default:<code>false</code>.</p></li><li><p><code>rejectUnauthorized</code>: If <code>true</code> the server will reject any connectionwhich is not authorized with the list of supplied CAs. This option onlyhas an effect if <code>requestCert</code> is <code>true</code>. Default: <code>false</code>.</p></li><li><p><code>NPNProtocols</code>: An array or <code>Buffer</code> of possible NPN protocols. (Protocolsshould be ordered by their priority).</p></li><li><p><code>SNICallback</code>: A function that will be called if client supports SNI TLSextension. Only one argument will be passed to it: <code>servername</code>. And<code>SNICallback</code> should return SecureContext instance.(You can use <code>crypto.createCredentials(...).context</code> to get properSecureContext). If <code>SNICallback</code> wasn&#39;t provided - default callback withhigh-level API will be used (see below).</p></li><li><p><code>sessionIdContext</code>: A string containing a opaque identifier for sessionresumption. If <code>requestCert</code> is <code>true</code>, the default is MD5 hash valuegenerated from command-line. Otherwise, the default is not provided.</p></li></ul><p>Here is a simple example echo server:</p><pre><code>var tls = require(&#39;tls&#39;);var fs = require(&#39;fs&#39;);var options = {  key: fs.readFileSync(&#39;server-key.pem&#39;),  cert: fs.readFileSync(&#39;server-cert.pem&#39;),  // This is necessary only if using the client certificate authentication.  requestCert: true,  // This is necessary only if the client uses the self-signed certificate.  ca: [ fs.readFileSync(&#39;client-cert.pem&#39;) ]};var server = tls.createServer(options, function(cleartextStream) {  console.log(&#39;server connected&#39;,              cleartextStream.authorized ? &#39;authorized&#39; : &#39;unauthorized&#39;);  cleartextStream.write(&quot;welcome!\n&quot;);  cleartextStream.setEncoding(&#39;utf8&#39;);  cleartextStream.pipe(cleartextStream);});server.listen(8000, function() {  console.log(&#39;server bound&#39;);});</code></pre><p>Or</p><pre><code>var tls = require(&#39;tls&#39;);var fs = require(&#39;fs&#39;);var options = {  pfx: fs.readFileSync(&#39;server.pfx&#39;),  // This is necessary only if using the client certificate authentication.  requestCert: true,};var server = tls.createServer(options, function(cleartextStream) {  console.log(&#39;server connected&#39;,              cleartextStream.authorized ? &#39;authorized&#39; : &#39;unauthorized&#39;);  cleartextStream.write(&quot;welcome!\n&quot;);  cleartextStream.setEncoding(&#39;utf8&#39;);  cleartextStream.pipe(cleartextStream);});server.listen(8000, function() {  console.log(&#39;server bound&#39;);});</code></pre><p>You can test this server by connecting to it with <code>openssl s_client</code>:</p><pre><code>openssl s_client -connect 127.0.0.1:8000</code></pre></summary>
        /// <signature>
        /// <param name="options"></param>
        /// <param name="secureConnectionListener"></param>
        /// </signature>
        return new tls_.Server();
    }
    this.connect = function(port, host, options, callback) {
        /// <summary><p>Creates a new client connection to the given <code>port</code> and <code>host</code> (old API) or<code>options.port</code> and <code>options.host</code>. (If <code>host</code> is omitted, it defaults to<code>localhost</code>.) <code>options</code> should be an object which specifies:</p><ul><li><p><code>host</code>: Host the client should connect to</p></li><li><p><code>port</code>: Port the client should connect to</p></li><li><p><code>socket</code>: Establish secure connection on a given socket rather thancreating a new socket. If this option is specified, <code>host</code> and <code>port</code>are ignored.</p></li><li><p><code>pfx</code>: A string or <code>Buffer</code> containing the private key, certificate andCA certs of the server in PFX or PKCS12 format.</p></li><li><p><code>key</code>: A string or <code>Buffer</code> containing the private key of the client inPEM format.</p></li><li><p><code>passphrase</code>: A string of passphrase for the private key or pfx.</p></li><li><p><code>cert</code>: A string or <code>Buffer</code> containing the certificate key of the client inPEM format.</p></li><li><p><code>ca</code>: An array of strings or <code>Buffer</code>s of trusted certificates. If this isomitted several well known &quot;root&quot; CAs will be used, like VeriSign.These are used to authorize connections.</p></li><li><p><code>rejectUnauthorized</code>: If <code>true</code>, the server certificate is verified againstthe list of supplied CAs. An <code>&#39;error&#39;</code> event is emitted if verificationfails. Default: <code>false</code>.</p></li><li><p><code>NPNProtocols</code>: An array of string or <code>Buffer</code> containing supported NPNprotocols. <code>Buffer</code> should have following format: <code>0x05hello0x05world</code>,where first byte is next protocol name&#39;s length. (Passing array shouldusually be much simpler: <code>[&#39;hello&#39;, &#39;world&#39;]</code>.)</p></li><li><p><code>servername</code>: Servername for SNI (Server Name Indication) TLS extension.</p></li></ul><p>The <code>callback</code> parameter will be added as a listener for the[&#39;secureConnect&#39;][] event.</p><p><code>tls.connect()</code> returns a [CleartextStream][] object.</p><p>Here is an example of a client of echo server as described previously:</p><pre><code>var tls = require(&#39;tls&#39;);var fs = require(&#39;fs&#39;);var options = {  // These are necessary only if using the client certificate authentication  key: fs.readFileSync(&#39;client-key.pem&#39;),  cert: fs.readFileSync(&#39;client-cert.pem&#39;),  // This is necessary only if the server uses the self-signed certificate  ca: [ fs.readFileSync(&#39;server-cert.pem&#39;) ]};var cleartextStream = tls.connect(8000, options, function() {  console.log(&#39;client connected&#39;,              cleartextStream.authorized ? &#39;authorized&#39; : &#39;unauthorized&#39;);  process.stdin.pipe(cleartextStream);  process.stdin.resume();});cleartextStream.setEncoding(&#39;utf8&#39;);cleartextStream.on(&#39;data&#39;, function(data) {  console.log(data);});cleartextStream.on(&#39;end&#39;, function() {  server.close();});</code></pre><p>Or</p><pre><code>var tls = require(&#39;tls&#39;);var fs = require(&#39;fs&#39;);var options = {  pfx: fs.readFileSync(&#39;client.pfx&#39;)};var cleartextStream = tls.connect(8000, options, function() {  console.log(&#39;client connected&#39;,              cleartextStream.authorized ? &#39;authorized&#39; : &#39;unauthorized&#39;);  process.stdin.pipe(cleartextStream);  process.stdin.resume();});cleartextStream.setEncoding(&#39;utf8&#39;);cleartextStream.on(&#39;data&#39;, function(data) {  console.log(data);});cleartextStream.on(&#39;end&#39;, function() {  server.close();});</code></pre></summary>
        /// <signature>
        /// <param name="port"></param>
        /// <param name="host"></param>
        /// <param name="options"></param>
        /// <param name="callback"></param>
        /// </signature>
        /// <signature>
        /// <param name="options"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.connect = function(port, host, options, callback) {
        /// <summary><p>Creates a new client connection to the given <code>port</code> and <code>host</code> (old API) or<code>options.port</code> and <code>options.host</code>. (If <code>host</code> is omitted, it defaults to<code>localhost</code>.) <code>options</code> should be an object which specifies:</p><ul><li><p><code>host</code>: Host the client should connect to</p></li><li><p><code>port</code>: Port the client should connect to</p></li><li><p><code>socket</code>: Establish secure connection on a given socket rather thancreating a new socket. If this option is specified, <code>host</code> and <code>port</code>are ignored.</p></li><li><p><code>pfx</code>: A string or <code>Buffer</code> containing the private key, certificate andCA certs of the server in PFX or PKCS12 format.</p></li><li><p><code>key</code>: A string or <code>Buffer</code> containing the private key of the client inPEM format.</p></li><li><p><code>passphrase</code>: A string of passphrase for the private key or pfx.</p></li><li><p><code>cert</code>: A string or <code>Buffer</code> containing the certificate key of the client inPEM format.</p></li><li><p><code>ca</code>: An array of strings or <code>Buffer</code>s of trusted certificates. If this isomitted several well known &quot;root&quot; CAs will be used, like VeriSign.These are used to authorize connections.</p></li><li><p><code>rejectUnauthorized</code>: If <code>true</code>, the server certificate is verified againstthe list of supplied CAs. An <code>&#39;error&#39;</code> event is emitted if verificationfails. Default: <code>false</code>.</p></li><li><p><code>NPNProtocols</code>: An array of string or <code>Buffer</code> containing supported NPNprotocols. <code>Buffer</code> should have following format: <code>0x05hello0x05world</code>,where first byte is next protocol name&#39;s length. (Passing array shouldusually be much simpler: <code>[&#39;hello&#39;, &#39;world&#39;]</code>.)</p></li><li><p><code>servername</code>: Servername for SNI (Server Name Indication) TLS extension.</p></li></ul><p>The <code>callback</code> parameter will be added as a listener for the[&#39;secureConnect&#39;][] event.</p><p><code>tls.connect()</code> returns a [CleartextStream][] object.</p><p>Here is an example of a client of echo server as described previously:</p><pre><code>var tls = require(&#39;tls&#39;);var fs = require(&#39;fs&#39;);var options = {  // These are necessary only if using the client certificate authentication  key: fs.readFileSync(&#39;client-key.pem&#39;),  cert: fs.readFileSync(&#39;client-cert.pem&#39;),  // This is necessary only if the server uses the self-signed certificate  ca: [ fs.readFileSync(&#39;server-cert.pem&#39;) ]};var cleartextStream = tls.connect(8000, options, function() {  console.log(&#39;client connected&#39;,              cleartextStream.authorized ? &#39;authorized&#39; : &#39;unauthorized&#39;);  process.stdin.pipe(cleartextStream);  process.stdin.resume();});cleartextStream.setEncoding(&#39;utf8&#39;);cleartextStream.on(&#39;data&#39;, function(data) {  console.log(data);});cleartextStream.on(&#39;end&#39;, function() {  server.close();});</code></pre><p>Or</p><pre><code>var tls = require(&#39;tls&#39;);var fs = require(&#39;fs&#39;);var options = {  pfx: fs.readFileSync(&#39;client.pfx&#39;)};var cleartextStream = tls.connect(8000, options, function() {  console.log(&#39;client connected&#39;,              cleartextStream.authorized ? &#39;authorized&#39; : &#39;unauthorized&#39;);  process.stdin.pipe(cleartextStream);  process.stdin.resume();});cleartextStream.setEncoding(&#39;utf8&#39;);cleartextStream.on(&#39;data&#39;, function(data) {  console.log(data);});cleartextStream.on(&#39;end&#39;, function() {  server.close();});</code></pre></summary>
        /// <signature>
        /// <param name="port"></param>
        /// <param name="host"></param>
        /// <param name="options"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.createSecurePair = function(credentials, isServer, requestCert, rejectUnauthorized) {
        /// <summary><p>Creates a new secure pair object with two streams, one of which reads/writesencrypted data, and one reads/writes cleartext data.Generally the encrypted one is piped to/from an incoming encrypted data stream,and the cleartext one is used as a replacement for the initial encrypted stream.</p><ul><li><p><code>credentials</code>: A credentials object from crypto.createCredentials( ... )</p></li><li><p><code>isServer</code>: A boolean indicating whether this tls connection should beopened as a server or a client.</p></li><li><p><code>requestCert</code>: A boolean indicating whether a server should request acertificate from a connecting client. Only applies to server connections.</p></li><li><p><code>rejectUnauthorized</code>: A boolean indicating whether a server shouldautomatically reject clients with invalid certificates. Only applies toservers with <code>requestCert</code> enabled.</p></li></ul><p><code>tls.createSecurePair()</code> returns a SecurePair object with [cleartext][] and<code>encrypted</code> stream properties.</p></summary>
        /// <signature>
        /// <param name="credentials"></param>
        /// <param name="isServer"></param>
        /// <param name="requestCert"></param>
        /// <param name="rejectUnauthorized"></param>
        /// </signature>
        return new tls_.SecurePair();
    }
    this.SecurePair = function() {
        emitter = new Events().EventEmitter;
        /// <field name='secure'><p>The event is emitted from the SecurePair once the pair has successfullyestablished a secure connection.</p><p>Similarly to the checking for the server &#39;secureConnection&#39; event,pair.cleartext.authorized should be checked to confirm whether the certificateused properly authorized.</p></field>
        this.secure = new emitter();
    }
    this.Server = function() {
        this.listen = function(port, host, callback) {
            /// <summary><p>Begin accepting connections on the specified <code>port</code> and <code>host</code>.  If the<code>host</code> is omitted, the server will accept connections directed to anyIPv4 address (<code>INADDR_ANY</code>).</p><p>This function is asynchronous. The last parameter <code>callback</code> will be calledwhen the server has been bound.</p><p>See <code>net.Server</code> for more information.</p></summary>
            /// <signature>
            /// <param name="port"></param>
            /// <param name="host"></param>
            /// <param name="callback"></param>
            /// </signature>
        }
        this.close = function() {
            /// <summary><p>Stops the server from accepting new connections. This function isasynchronous, the server is finally closed when the server emits a <code>&#39;close&#39;</code>event.</p></summary>
        }
        this.address = function() {
            /// <summary><p>Returns the bound address, the address family name and port of theserver as reported by the operating system.  See [net.Server.address()][] formore information.</p></summary>
        }
        this.addContext = function(hostname, credentials) {
            /// <summary><p>Add secure context that will be used if client request&#39;s SNI hostname ismatching passed <code>hostname</code> (wildcards can be used). <code>credentials</code> can contain<code>key</code>, <code>cert</code> and <code>ca</code>.</p></summary>
            /// <signature>
            /// <param name="hostname"></param>
            /// <param name="credentials"></param>
            /// </signature>
        }
        emitter = new Events().EventEmitter;
        /// <field name='secureConnection'><p><code>function (cleartextStream) {}</code></p><p>This event is emitted after a new connection has been successfullyhandshaked. The argument is a instance of [CleartextStream][]. It has all thecommon stream methods and events.</p><p><code>cleartextStream.authorized</code> is a boolean value which indicates if theclient has verified by one of the supplied certificate authorities for theserver. If <code>cleartextStream.authorized</code> is false, then<code>cleartextStream.authorizationError</code> is set to describe how authorizationfailed. Implied but worth mentioning: depending on the settings of the TLSserver, you unauthorized connections may be accepted.<code>cleartextStream.npnProtocol</code> is a string containing selected NPN protocol.<code>cleartextStream.servername</code> is a string containing servername requested withSNI.</p></field>
        this.secureConnection = new emitter();
        /// <field name='clientError'><p><code>function (exception) { }</code></p><p>When a client connection emits an &#39;error&#39; event before secure connection isestablished - it will be forwarded here.</p></field>
        this.clientError = new emitter();
        /// <field name='maxConnections'><p>Set this property to reject connections when the server&#39;s connection countgets high.</p></field>
        this.maxConnections = undefined;
        /// <field name='connections'><p>The number of concurrent connections on the server.</p></field>
        this.connections = undefined;
    }
    this.CleartextStream = function() {
        this.getPeerCertificate = function() {
            /// <summary><p>Returns an object representing the peer&#39;s certificate. The returned object hassome properties corresponding to the field of the certificate.</p><p>Example:</p><pre><code>{ subject:    { C: &#39;UK&#39;,     ST: &#39;Acknack Ltd&#39;,     L: &#39;Rhys Jones&#39;,     O: &#39;node.js&#39;,     OU: &#39;Test TLS Certificate&#39;,     CN: &#39;localhost&#39; },  issuer:    { C: &#39;UK&#39;,     ST: &#39;Acknack Ltd&#39;,     L: &#39;Rhys Jones&#39;,     O: &#39;node.js&#39;,     OU: &#39;Test TLS Certificate&#39;,     CN: &#39;localhost&#39; },  valid_from: &#39;Nov 11 09:52:22 2009 GMT&#39;,  valid_to: &#39;Nov  6 09:52:22 2029 GMT&#39;,  fingerprint: &#39;2A:7A:C2:DD:E5:F9:CC:53:72:35:99:7A:02:5A:71:38:52:EC:8A:DF&#39; }</code></pre><p>If the peer does not provide a certificate, it returns <code>null</code> or an emptyobject.</p></summary>
        }
        this.getCipher = function() {
            /// <summary><p>Returns an object representing the cipher name and the SSL/TLSprotocol version of the current connection.</p><p>Example:{ name: &#39;AES256-SHA&#39;, version: &#39;TLSv1/SSLv3&#39; }</p><p>See SSL_CIPHER_get_name() and SSL_CIPHER_get_version() in<a href="http://www.openssl.org/docs/ssl/ssl.html#DEALING_WITH_CIPHERS">http://www.openssl.org/docs/ssl/ssl.html#DEALING_WITH_CIPHERS</a> for moreinformation.</p></summary>
        }
        this.address = function() {
            /// <summary><p>Returns the bound address, the address family name and port of theunderlying socket as reported by the operating system. Returns anobject with three properties, e.g.<code>{ port: 12346, family: &#39;IPv4&#39;, address: &#39;127.0.0.1&#39; }</code></p></summary>
        }
        emitter = new Events().EventEmitter;
        /// <field name='secureConnect'><p>This event is emitted after a new connection has been successfully handshaked. The listener will be called no matter if the server&#39;s certificate wasauthorized or not. It is up to the user to test <code>cleartextStream.authorized</code>to see if the server certificate was signed by one of the specified CAs.If <code>cleartextStream.authorized === false</code> then the error can be found in<code>cleartextStream.authorizationError</code>. Also if NPN was used - you can check<code>cleartextStream.npnProtocol</code> for negotiated protocol.</p></field>
        this.secureConnect = new emitter();
        /// <field name='authorized'><p>A boolean that is <code>true</code> if the peer certificate was signed by one of thespecified CAs, otherwise <code>false</code></p></field>
        this.authorized = undefined;
        /// <field name='authorizationError'><p>The reason why the peer&#39;s certificate has not been verified. This propertybecomes available only when <code>cleartextStream.authorized === false</code>.</p></field>
        this.authorizationError = undefined;
        /// <field name='remoteAddress'><p>The string representation of the remote IP address. For example,<code>&#39;74.125.127.100&#39;</code> or <code>&#39;2001:4860:a005::68&#39;</code>.</p></field>
        this.remoteAddress = undefined;
        /// <field name='remotePort'><p>The numeric representation of the remote port. For example, <code>443</code>.</p></field>
        this.remotePort = undefined;
    }
} // module tls_(ssl)

function stringdecoder() {
    /// <summary><p>To use this module, do <code>require(&#39;string_decoder&#39;)</code>. StringDecoder decodes abuffer to a string. It is a simple interface to <code>buffer.toString()</code> but providesadditional support for utf8.</p><pre><code>var StringDecoder = require(&#39;string_decoder&#39;).StringDecoder;var decoder = new StringDecoder(&#39;utf8&#39;);var cent = new Buffer([0xC2, 0xA2]);console.log(decoder.write(cent));var euro = new Buffer([0xE2, 0x82, 0xAC]);console.log(decoder.write(euro));</code></pre></summary>
    this.StringDecoder = function() {
        this.write = function(buffer) {
            /// <summary><p>Returns a decoded string.</p></summary>
            /// <signature>
            /// <param name="buffer"></param>
            /// </signature>
        }
    }
} // module stringdecoder

function fs() {
    /// <summary><p>File I/O is provided by simple wrappers around standard POSIX functions.  Touse this module do <code>require(&#39;fs&#39;)</code>. All the methods have asynchronous andsynchronous forms.</p><p>The asynchronous form always take a completion callback as its last argument.The arguments passed to the completion callback depend on the method, but thefirst argument is always reserved for an exception. If the operation wascompleted successfully, then the first argument will be <code>null</code> or <code>undefined</code>.</p><p>When using the synchronous form any exceptions are immediately thrown.You can use try/catch to handle exceptions or allow them to bubble up.</p><p>Here is an example of the asynchronous version:</p><pre><code>var fs = require(&#39;fs&#39;);fs.unlink(&#39;/tmp/hello&#39;, function (err) {  if (err) throw err;  console.log(&#39;successfully deleted /tmp/hello&#39;);});</code></pre><p>Here is the synchronous version:</p><pre><code>var fs = require(&#39;fs&#39;);fs.unlinkSync(&#39;/tmp/hello&#39;)console.log(&#39;successfully deleted /tmp/hello&#39;);</code></pre><p>With the asynchronous methods there is no guaranteed ordering. So thefollowing is prone to error:</p><pre><code>fs.rename(&#39;/tmp/hello&#39;, &#39;/tmp/world&#39;, function (err) {  if (err) throw err;  console.log(&#39;renamed complete&#39;);});fs.stat(&#39;/tmp/world&#39;, function (err, stats) {  if (err) throw err;  console.log(&#39;stats: &#39; + JSON.stringify(stats));});</code></pre><p>It could be that <code>fs.stat</code> is executed before <code>fs.rename</code>.The correct way to do this is to chain the callbacks.</p><pre><code>fs.rename(&#39;/tmp/hello&#39;, &#39;/tmp/world&#39;, function (err) {  if (err) throw err;  fs.stat(&#39;/tmp/world&#39;, function (err, stats) {    if (err) throw err;    console.log(&#39;stats: &#39; + JSON.stringify(stats));  });});</code></pre><p>In busy processes, the programmer is <em>strongly encouraged</em> to use theasynchronous versions of these calls. The synchronous versions will blockthe entire process until they complete--halting all connections.</p><p>Relative path to filename can be used, remember however that this path will be relativeto <code>process.cwd()</code>.</p></summary>
    this.rename = function(oldPath, newPath, callback) {
        /// <summary><p>Asynchronous rename(2). No arguments other than a possible exception are givento the completion callback.</p></summary>
        /// <signature>
        /// <param name="oldPath"></param>
        /// <param name="newPath"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.renameSync = function(oldPath, newPath) {
        /// <summary><p>Synchronous rename(2).</p></summary>
        /// <signature>
        /// <param name="oldPath"></param>
        /// <param name="newPath"></param>
        /// </signature>
    }
    this.truncate = function(fd, len, callback) {
        /// <summary><p>Asynchronous ftruncate(2). No arguments other than a possible exception aregiven to the completion callback.</p></summary>
        /// <signature>
        /// <param name="fd"></param>
        /// <param name="len"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.truncateSync = function(fd, len) {
        /// <summary><p>Synchronous ftruncate(2).</p></summary>
        /// <signature>
        /// <param name="fd"></param>
        /// <param name="len"></param>
        /// </signature>
    }
    this.chown = function(path, uid, gid, callback) {
        /// <summary><p>Asynchronous chown(2). No arguments other than a possible exception are givento the completion callback.</p></summary>
        /// <signature>
        /// <param name="path"></param>
        /// <param name="uid"></param>
        /// <param name="gid"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.chownSync = function(path, uid, gid) {
        /// <summary><p>Synchronous chown(2).</p></summary>
        /// <signature>
        /// <param name="path"></param>
        /// <param name="uid"></param>
        /// <param name="gid"></param>
        /// </signature>
    }
    this.fchown = function(fd, uid, gid, callback) {
        /// <summary><p>Asynchronous fchown(2). No arguments other than a possible exception are givento the completion callback.</p></summary>
        /// <signature>
        /// <param name="fd"></param>
        /// <param name="uid"></param>
        /// <param name="gid"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.fchownSync = function(fd, uid, gid) {
        /// <summary><p>Synchronous fchown(2).</p></summary>
        /// <signature>
        /// <param name="fd"></param>
        /// <param name="uid"></param>
        /// <param name="gid"></param>
        /// </signature>
    }
    this.lchown = function(path, uid, gid, callback) {
        /// <summary><p>Asynchronous lchown(2). No arguments other than a possible exception are givento the completion callback.</p></summary>
        /// <signature>
        /// <param name="path"></param>
        /// <param name="uid"></param>
        /// <param name="gid"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.lchownSync = function(path, uid, gid) {
        /// <summary><p>Synchronous lchown(2).</p></summary>
        /// <signature>
        /// <param name="path"></param>
        /// <param name="uid"></param>
        /// <param name="gid"></param>
        /// </signature>
    }
    this.chmod = function(path, mode, callback) {
        /// <summary><p>Asynchronous chmod(2). No arguments other than a possible exception are givento the completion callback.</p></summary>
        /// <signature>
        /// <param name="path"></param>
        /// <param name="mode"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.chmodSync = function(path, mode) {
        /// <summary><p>Synchronous chmod(2).</p></summary>
        /// <signature>
        /// <param name="path"></param>
        /// <param name="mode"></param>
        /// </signature>
    }
    this.fchmod = function(fd, mode, callback) {
        /// <summary><p>Asynchronous fchmod(2). No arguments other than a possible exceptionare given to the completion callback.</p></summary>
        /// <signature>
        /// <param name="fd"></param>
        /// <param name="mode"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.fchmodSync = function(fd, mode) {
        /// <summary><p>Synchronous fchmod(2).</p></summary>
        /// <signature>
        /// <param name="fd"></param>
        /// <param name="mode"></param>
        /// </signature>
    }
    this.lchmod = function(path, mode, callback) {
        /// <summary><p>Asynchronous lchmod(2). No arguments other than a possible exceptionare given to the completion callback.</p><p>Only available on Mac OS X.</p></summary>
        /// <signature>
        /// <param name="path"></param>
        /// <param name="mode"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.lchmodSync = function(path, mode) {
        /// <summary><p>Synchronous lchmod(2).</p></summary>
        /// <signature>
        /// <param name="path"></param>
        /// <param name="mode"></param>
        /// </signature>
    }
    this.stat = function(path, callback) {
        /// <summary><p>Asynchronous stat(2). The callback gets two arguments <code>(err, stats)</code> where<code>stats</code> is a <a href="#fs_class_fs_stats">fs.Stats</a> object.  See the <a href="#fs_class_fs_stats">fs.Stats</a>section below for more information.</p></summary>
        /// <signature>
        /// <param name="path"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.lstat = function(path, callback) {
        /// <summary><p>Asynchronous lstat(2). The callback gets two arguments <code>(err, stats)</code> where<code>stats</code> is a <code>fs.Stats</code> object. <code>lstat()</code> is identical to <code>stat()</code>, except that if<code>path</code> is a symbolic link, then the link itself is stat-ed, not the file that itrefers to.</p></summary>
        /// <signature>
        /// <param name="path"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.fstat = function(fd, callback) {
        /// <summary><p>Asynchronous fstat(2). The callback gets two arguments <code>(err, stats)</code> where<code>stats</code> is a <code>fs.Stats</code> object. <code>fstat()</code> is identical to <code>stat()</code>, except thatthe file to be stat-ed is specified by the file descriptor <code>fd</code>.</p></summary>
        /// <signature>
        /// <param name="fd"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.statSync = function(path) {
        /// <summary><p>Synchronous stat(2). Returns an instance of <code>fs.Stats</code>.</p></summary>
        /// <signature>
        /// <param name="path"></param>
        /// </signature>
    }
    this.lstatSync = function(path) {
        /// <summary><p>Synchronous lstat(2). Returns an instance of <code>fs.Stats</code>.</p></summary>
        /// <signature>
        /// <param name="path"></param>
        /// </signature>
    }
    this.fstatSync = function(fd) {
        /// <summary><p>Synchronous fstat(2). Returns an instance of <code>fs.Stats</code>.</p></summary>
        /// <signature>
        /// <param name="fd"></param>
        /// </signature>
    }
    this.link = function(srcpath, dstpath, callback) {
        /// <summary><p>Asynchronous link(2). No arguments other than a possible exception are given tothe completion callback.</p></summary>
        /// <signature>
        /// <param name="srcpath"></param>
        /// <param name="dstpath"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.linkSync = function(srcpath, dstpath) {
        /// <summary><p>Synchronous link(2).</p></summary>
        /// <signature>
        /// <param name="srcpath"></param>
        /// <param name="dstpath"></param>
        /// </signature>
    }
    this.symlink = function(srcpath, dstpath, type, callback) {
        /// <summary><p>Asynchronous symlink(2). No arguments other than a possible exception are givento the completion callback.<code>type</code> argument can be either <code>&#39;dir&#39;</code>, <code>&#39;file&#39;</code>, or <code>&#39;junction&#39;</code> (default is <code>&#39;file&#39;</code>).  It is only used on Windows (ignored on other platforms).Note that Windows junction points require the destination path to be absolute.  When using<code>&#39;junction&#39;</code>, the <code>destination</code> argument will automatically be normalized to absolute path.</p></summary>
        /// <signature>
        /// <param name="srcpath"></param>
        /// <param name="dstpath"></param>
        /// <param name="type"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.symlinkSync = function(srcpath, dstpath, type) {
        /// <summary><p>Synchronous symlink(2).</p></summary>
        /// <signature>
        /// <param name="srcpath"></param>
        /// <param name="dstpath"></param>
        /// <param name="type"></param>
        /// </signature>
    }
    this.readlink = function(path, callback) {
        /// <summary><p>Asynchronous readlink(2). The callback gets two arguments <code>(err,linkString)</code>.</p></summary>
        /// <signature>
        /// <param name="path"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.readlinkSync = function(path) {
        /// <summary><p>Synchronous readlink(2). Returns the symbolic link&#39;s string value.</p></summary>
        /// <signature>
        /// <param name="path"></param>
        /// </signature>
    }
    this.realpath = function(path, cache, callback) {
        /// <summary><p>Asynchronous realpath(2). The <code>callback</code> gets two arguments <code>(err,resolvedPath)</code>. May use <code>process.cwd</code> to resolve relative paths. <code>cache</code> is anobject literal of mapped paths that can be used to force a specific pathresolution or avoid additional <code>fs.stat</code> calls for known real paths.</p><p>Example:</p><pre><code>var cache = {&#39;/etc&#39;:&#39;/private/etc&#39;};fs.realpath(&#39;/etc/passwd&#39;, cache, function (err, resolvedPath) {  if (err) throw err;  console.log(resolvedPath);});</code></pre></summary>
        /// <signature>
        /// <param name="path"></param>
        /// <param name="cache"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.realpathSync = function(path, cache) {
        /// <summary><p>Synchronous realpath(2). Returns the resolved path.</p></summary>
        /// <signature>
        /// <param name="path"></param>
        /// <param name="cache"></param>
        /// </signature>
    }
    this.unlink = function(path, callback) {
        /// <summary><p>Asynchronous unlink(2). No arguments other than a possible exception are givento the completion callback.</p></summary>
        /// <signature>
        /// <param name="path"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.unlinkSync = function(path) {
        /// <summary><p>Synchronous unlink(2).</p></summary>
        /// <signature>
        /// <param name="path"></param>
        /// </signature>
    }
    this.rmdir = function(path, callback) {
        /// <summary><p>Asynchronous rmdir(2). No arguments other than a possible exception are givento the completion callback.</p></summary>
        /// <signature>
        /// <param name="path"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.rmdirSync = function(path) {
        /// <summary><p>Synchronous rmdir(2).</p></summary>
        /// <signature>
        /// <param name="path"></param>
        /// </signature>
    }
    this.mkdir = function(path, mode, callback) {
        /// <summary><p>Asynchronous mkdir(2). No arguments other than a possible exception are givento the completion callback. <code>mode</code> defaults to <code>0777</code>.</p></summary>
        /// <signature>
        /// <param name="path"></param>
        /// <param name="mode"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.mkdirSync = function(path, mode) {
        /// <summary><p>Synchronous mkdir(2).</p></summary>
        /// <signature>
        /// <param name="path"></param>
        /// <param name="mode"></param>
        /// </signature>
    }
    this.readdir = function(path, callback) {
        /// <summary><p>Asynchronous readdir(3).  Reads the contents of a directory.The callback gets two arguments <code>(err, files)</code> where <code>files</code> is an array ofthe names of the files in the directory excluding <code>&#39;.&#39;</code> and <code>&#39;..&#39;</code>.</p></summary>
        /// <signature>
        /// <param name="path"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.readdirSync = function(path) {
        /// <summary><p>Synchronous readdir(3). Returns an array of filenames excluding <code>&#39;.&#39;</code> and<code>&#39;..&#39;</code>.</p></summary>
        /// <signature>
        /// <param name="path"></param>
        /// </signature>
    }
    this.close = function(fd, callback) {
        /// <summary><p>Asynchronous close(2).  No arguments other than a possible exception are givento the completion callback.</p></summary>
        /// <signature>
        /// <param name="fd"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.closeSync = function(fd) {
        /// <summary><p>Synchronous close(2).</p></summary>
        /// <signature>
        /// <param name="fd"></param>
        /// </signature>
    }
    this.open = function(path, flags, mode, callback) {
        /// <summary><p>Asynchronous file open. See open(2). <code>flags</code> can be:</p><ul><li><p><code>&#39;r&#39;</code> - Open file for reading.An exception occurs if the file does not exist.</p></li><li><p><code>&#39;r+&#39;</code> - Open file for reading and writing.An exception occurs if the file does not exist.</p></li><li><p><code>&#39;rs&#39;</code> - Open file for reading in synchronous mode. Instructs the operatingsystem to bypass the local file system cache.</p><p>This is primarily useful for opening files on NFS mounts as it allows you toskip the potentially stale local cache. It has a very real impact on I/Operformance so don&#39;t use this mode unless you need it.</p><p>Note that this doesn&#39;t turn <code>fs.open()</code> into a synchronous blocking call.If that&#39;s what you want then you should be using <code>fs.openSync()</code></p></li><li><p><code>&#39;rs+&#39;</code> - Open file for reading and writing, telling the OS to open itsynchronously. See notes for <code>&#39;rs&#39;</code> about using this with caution.</p></li><li><p><code>&#39;w&#39;</code> - Open file for writing.The file is created (if it does not exist) or truncated (if it exists).</p></li><li><p><code>&#39;wx&#39;</code> - Like <code>&#39;w&#39;</code> but opens the file in exclusive mode.</p></li><li><p><code>&#39;w+&#39;</code> - Open file for reading and writing.The file is created (if it does not exist) or truncated (if it exists).</p></li><li><p><code>&#39;wx+&#39;</code> - Like <code>&#39;w+&#39;</code> but opens the file in exclusive mode.</p></li><li><p><code>&#39;a&#39;</code> - Open file for appending.The file is created if it does not exist.</p></li><li><p><code>&#39;ax&#39;</code> - Like <code>&#39;a&#39;</code> but opens the file in exclusive mode.</p></li><li><p><code>&#39;a+&#39;</code> - Open file for reading and appending.The file is created if it does not exist.</p></li><li><p><code>&#39;ax+&#39;</code> - Like <code>&#39;a+&#39;</code> but opens the file in exclusive mode.</p></li></ul><p><code>mode</code> defaults to <code>0666</code>. The callback gets two arguments <code>(err, fd)</code>.</p><p>Exclusive mode (<code>O_EXCL</code>) ensures that <code>path</code> is newly created. <code>fs.open()</code>fails if a file by that name already exists. On POSIX systems, symlinks arenot followed. Exclusive mode may or may not work with network file systems.</p></summary>
        /// <signature>
        /// <param name="path"></param>
        /// <param name="flags"></param>
        /// <param name="mode"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.openSync = function(path, flags, mode) {
        /// <summary><p>Synchronous open(2).</p></summary>
        /// <signature>
        /// <param name="path"></param>
        /// <param name="flags"></param>
        /// <param name="mode"></param>
        /// </signature>
    }
    this.utimes = function(path, atime, mtime) {
        /// <summary><p>Change file timestamps of the file referenced by the supplied path.</p></summary>
        /// <signature>
        /// <param name="path"></param>
        /// <param name="atime"></param>
        /// <param name="mtime"></param>
        /// </signature>
        /// <signature>
        /// <param name="path"></param>
        /// <param name="atime"></param>
        /// <param name="mtime"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.utimesSync = function(path, atime, mtime) {
        /// <summary><p>Change file timestamps of the file referenced by the supplied path.</p></summary>
        /// <signature>
        /// <param name="path"></param>
        /// <param name="atime"></param>
        /// <param name="mtime"></param>
        /// </signature>
    }
    this.futimes = function(fd, atime, mtime) {
        /// <summary><p>Change the file timestamps of a file referenced by the supplied filedescriptor.</p></summary>
        /// <signature>
        /// <param name="fd"></param>
        /// <param name="atime"></param>
        /// <param name="mtime"></param>
        /// </signature>
        /// <signature>
        /// <param name="fd"></param>
        /// <param name="atime"></param>
        /// <param name="mtime"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.futimesSync = function(fd, atime, mtime) {
        /// <summary><p>Change the file timestamps of a file referenced by the supplied filedescriptor.</p></summary>
        /// <signature>
        /// <param name="fd"></param>
        /// <param name="atime"></param>
        /// <param name="mtime"></param>
        /// </signature>
    }
    this.fsync = function(fd, callback) {
        /// <summary><p>Asynchronous fsync(2). No arguments other than a possible exception are givento the completion callback.</p></summary>
        /// <signature>
        /// <param name="fd"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.fsyncSync = function(fd) {
        /// <summary><p>Synchronous fsync(2).</p></summary>
        /// <signature>
        /// <param name="fd"></param>
        /// </signature>
    }
    this.write = function(fd, buffer, offset, length, position, callback) {
        /// <summary><p>Write <code>buffer</code> to the file specified by <code>fd</code>.</p><p><code>offset</code> and <code>length</code> determine the part of the buffer to be written.</p><p><code>position</code> refers to the offset from the beginning of the file where this datashould be written. If <code>position</code> is <code>null</code>, the data will be written at thecurrent position.See pwrite(2).</p><p>The callback will be given three arguments <code>(err, written, buffer)</code> where <code>written</code>specifies how many <em>bytes</em> were written from <code>buffer</code>.</p><p>Note that it is unsafe to use <code>fs.write</code> multiple times on the same filewithout waiting for the callback. For this scenario,<code>fs.createWriteStream</code> is strongly recommended.</p></summary>
        /// <signature>
        /// <param name="fd"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="position"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.writeSync = function(fd, buffer, offset, length, position) {
        /// <summary><p>Synchronous version of <code>fs.write()</code>. Returns the number of bytes written.</p></summary>
        /// <signature>
        /// <param name="fd"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="position"></param>
        /// </signature>
    }
    this.read = function(fd, buffer, offset, length, position, callback) {
        /// <summary><p>Read data from the file specified by <code>fd</code>.</p><p><code>buffer</code> is the buffer that the data will be written to.</p><p><code>offset</code> is offset within the buffer where writing will start.</p><p><code>length</code> is an integer specifying the number of bytes to read.</p><p><code>position</code> is an integer specifying where to begin reading from in the file.If <code>position</code> is <code>null</code>, data will be read from the current file position.</p><p>The callback is given the three arguments, <code>(err, bytesRead, buffer)</code>.</p></summary>
        /// <signature>
        /// <param name="fd"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="position"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.readSync = function(fd, buffer, offset, length, position) {
        /// <summary><p>Synchronous version of <code>fs.read</code>. Returns the number of <code>bytesRead</code>.</p></summary>
        /// <signature>
        /// <param name="fd"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="position"></param>
        /// </signature>
    }
    this.readFile = function(filename, encoding, callback) {
        /// <summary><p>Asynchronously reads the entire contents of a file. Example:</p><pre><code>fs.readFile(&#39;/etc/passwd&#39;, function (err, data) {  if (err) throw err;  console.log(data);});</code></pre><p>The callback is passed two arguments <code>(err, data)</code>, where <code>data</code> is thecontents of the file.</p><p>If no encoding is specified, then the raw buffer is returned.</p></summary>
        /// <signature>
        /// <param name="filename"></param>
        /// <param name="encoding"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.readFileSync = function(filename, encoding) {
        /// <summary><p>Synchronous version of <code>fs.readFile</code>. Returns the contents of the <code>filename</code>.</p><p>If <code>encoding</code> is specified then this function returns a string. Otherwise itreturns a buffer.</p></summary>
        /// <signature>
        /// <param name="filename"></param>
        /// <param name="encoding"></param>
        /// </signature>
    }
    this.writeFile = function(filename, data, encoding, callback) {
        /// <summary><p>Asynchronously writes data to a file, replacing the file if it already exists.<code>data</code> can be a string or a buffer. The <code>encoding</code> argument is ignored if<code>data</code> is a buffer. It defaults to <code>&#39;utf8&#39;</code>.</p><p>Example:</p><pre><code>fs.writeFile(&#39;message.txt&#39;, &#39;Hello Node&#39;, function (err) {  if (err) throw err;  console.log(&#39;It\&#39;s saved!&#39;);});</code></pre></summary>
        /// <signature>
        /// <param name="filename"></param>
        /// <param name="data"></param>
        /// <param name="encoding"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.writeFileSync = function(filename, data, encoding) {
        /// <summary><p>The synchronous version of <code>fs.writeFile</code>.</p></summary>
        /// <signature>
        /// <param name="filename"></param>
        /// <param name="data"></param>
        /// <param name="encoding"></param>
        /// </signature>
    }
    this.appendFile = function(filename, data, encoding, callback) {
        /// <summary><p>Asynchronously append data to a file, creating the file if it not yet exists.<code>data</code> can be a string or a buffer. The <code>encoding</code> argument is ignored if<code>data</code> is a buffer.</p><p>Example:</p><pre><code>fs.appendFile(&#39;message.txt&#39;, &#39;data to append&#39;, function (err) {  if (err) throw err;  console.log(&#39;The &quot;data to append&quot; was appended to file!&#39;);});</code></pre></summary>
        /// <signature>
        /// <param name="filename"></param>
        /// <param name="data"></param>
        /// <param name="encoding"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.appendFileSync = function(filename, data, encoding) {
        /// <summary><p>The synchronous version of <code>fs.appendFile</code>.</p></summary>
        /// <signature>
        /// <param name="filename"></param>
        /// <param name="data"></param>
        /// <param name="encoding"></param>
        /// </signature>
    }
    this.watchFile = function(filename, options, listener) {
        /// <summary><p>Watch for changes on <code>filename</code>. The callback <code>listener</code> will be called eachtime the file is accessed.</p><p>The second argument is optional. The <code>options</code> if provided should be an objectcontaining two members a boolean, <code>persistent</code>, and <code>interval</code>. <code>persistent</code>indicates whether the process should continue to run as long as files arebeing watched. <code>interval</code> indicates how often the target should be polled,in milliseconds. The default is <code>{ persistent: true, interval: 5007 }</code>.</p><p>The <code>listener</code> gets two arguments the current stat object and the previousstat object:</p><pre><code>fs.watchFile(&#39;message.text&#39;, function (curr, prev) {  console.log(&#39;the current mtime is: &#39; + curr.mtime);  console.log(&#39;the previous mtime was: &#39; + prev.mtime);});</code></pre><p>These stat objects are instances of <code>fs.Stat</code>.</p><p>If you want to be notified when the file was modified, not just accessedyou need to compare <code>curr.mtime</code> and <code>prev.mtime</code>.</p></summary>
        /// <signature>
        /// <param name="filename"></param>
        /// <param name="options"></param>
        /// <param name="listener"></param>
        /// </signature>
    }
    this.unwatchFile = function(filename, listener) {
        /// <summary><p>Stop watching for changes on <code>filename</code>. If <code>listener</code> is specified, only thatparticular listener is removed. Otherwise, <em>all</em> listeners are removed and youhave effectively stopped watching <code>filename</code>.</p><p>Calling <code>fs.unwatchFile()</code> with a filename that is not being watched is ano-op, not an error.</p></summary>
        /// <signature>
        /// <param name="filename"></param>
        /// <param name="listener"></param>
        /// </signature>
    }
    this.watch = function(filename, options, listener) {
        /// <summary><p>Watch for changes on <code>filename</code>, where <code>filename</code> is either a file or adirectory.  The returned object is a <a href="#fs_class_fs_fswatcher">fs.FSWatcher</a>.</p><p>The second argument is optional. The <code>options</code> if provided should be an objectcontaining a boolean member <code>persistent</code>, which indicates whether the processshould continue to run as long as files are being watched. The default is<code>{ persistent: true }</code>.</p><p>The listener callback gets two arguments <code>(event, filename)</code>.  <code>event</code> is either&#39;rename&#39; or &#39;change&#39;, and <code>filename</code> is the name of the file which triggeredthe event.</p></summary>
        /// <signature>
        /// <param name="filename"></param>
        /// <param name="options"></param>
        /// <param name="listener"></param>
        /// </signature>
    }
    this.exists = function(path, callback) {
        /// <summary><p>Test whether or not the given path exists by checking with the file system.Then call the <code>callback</code> argument with either true or false.  Example:</p><pre><code>fs.exists(&#39;/etc/passwd&#39;, function (exists) {  util.debug(exists ? &quot;it&#39;s there&quot; : &quot;no passwd!&quot;);});</code></pre></summary>
        /// <signature>
        /// <param name="path"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.existsSync = function(path) {
        /// <summary><p>Synchronous version of <code>fs.exists</code>.</p></summary>
        /// <signature>
        /// <param name="path"></param>
        /// </signature>
    }
    this.createReadStream = function(path, options) {
        /// <summary><p>Returns a new ReadStream object (See <code>Readable Stream</code>).</p><p><code>options</code> is an object with the following defaults:</p><pre><code>{ flags: &#39;r&#39;,  encoding: null,  fd: null,  mode: 0666,  bufferSize: 64 * 1024}</code></pre><p><code>options</code> can include <code>start</code> and <code>end</code> values to read a range of bytes fromthe file instead of the entire file.  Both <code>start</code> and <code>end</code> are inclusive andstart at 0. The <code>encoding</code> can be <code>&#39;utf8&#39;</code>, <code>&#39;ascii&#39;</code>, or <code>&#39;base64&#39;</code>.</p><p>An example to read the last 10 bytes of a file which is 100 bytes long:</p><pre><code>fs.createReadStream(&#39;sample.txt&#39;, {start: 90, end: 99});</code></pre></summary>
        /// <signature>
        /// <param name="path"></param>
        /// <param name="options"></param>
        /// </signature>
        return new fs.ReadStream();
    }
    this.createWriteStream = function(path, options) {
        /// <summary><p>Returns a new WriteStream object (See <code>Writable Stream</code>).</p><p><code>options</code> is an object with the following defaults:</p><pre><code>{ flags: &#39;w&#39;,  encoding: null,  mode: 0666 }</code></pre><p><code>options</code> may also include a <code>start</code> option to allow writing data atsome position past the beginning of the file.  Modifying a file ratherthan replacing it may require a <code>flags</code> mode of <code>r+</code> rather than thedefault mode <code>w</code>.</p></summary>
        /// <signature>
        /// <param name="path"></param>
        /// <param name="options"></param>
        /// </signature>
        return new fs.WriteStream();
    }
    this.Stats = function() {
    }
    this.ReadStream = function() {
        emitter = new Events().EventEmitter;
        /// <field name='open'><p>Emitted when the ReadStream&#39;s file is opened.</p></field>
        this.open = new emitter();
    }
    this.FSWatcher = function() {
        this.close = function() {
            /// <summary><p>Stop watching for changes on the given <code>fs.FSWatcher</code>.</p></summary>
        }
        emitter = new Events().EventEmitter;
        /// <field name='change'><p>Emitted when something changes in a watched directory or file.See more details in <a href="#fs_fs_watch_filename_options_listener">fs.watch</a>.</p></field>
        this.change = new emitter();
        /// <field name='error'><p>Emitted when an error occurs.</p></field>
        this.error = new emitter();
    }
    /// <field name='WriteStream'><p><code>WriteStream</code> is a <a href="stream.html#stream_writable_stream">Writable Stream</a>.</p></field>
    this.WriteStream = undefined;
} // module fs

function path() {
    /// <summary><p>This module contains utilities for handling and transforming filepaths.  Almost all these methods perform only string transformations.The file system is not consulted to check whether paths are valid.</p><p>Use <code>require(&#39;path&#39;)</code> to use this module.  The following methods are provided:</p></summary>
    this.normalize = function(p) {
        /// <summary><p>Normalize a string path, taking care of <code>&#39;..&#39;</code> and <code>&#39;.&#39;</code> parts.</p><p>When multiple slashes are found, they&#39;re replaced by a single one;when the path contains a trailing slash, it is preserved.On windows backslashes are used. </p><p>Example:</p><pre><code>path.normalize(&#39;/foo/bar//baz/asdf/quux/..&#39;)// returns&#39;/foo/bar/baz/asdf&#39;</code></pre></summary>
        /// <signature>
        /// <param name="p"></param>
        /// </signature>
    }
    this.join = function(path1, path2) {
        /// <summary><p>Join all arguments together and normalize the resulting path.Non-string arguments are ignored.</p><p>Example:</p><pre><code>path.join(&#39;/foo&#39;, &#39;bar&#39;, &#39;baz/asdf&#39;, &#39;quux&#39;, &#39;..&#39;)// returns&#39;/foo/bar/baz/asdf&#39;path.join(&#39;foo&#39;, {}, &#39;bar&#39;)// returns&#39;foo/bar&#39;</code></pre></summary>
        /// <signature>
        /// <param name="path1"></param>
        /// <param name="path2"></param>
        /// <param name="..."></param>
        /// </signature>
    }
    this.resolve = function(from, to) {
        /// <summary><p>Resolves <code>to</code> to an absolute path.</p><p>If <code>to</code> isn&#39;t already absolute <code>from</code> arguments are prepended in right to leftorder, until an absolute path is found. If after using all <code>from</code> paths stillno absolute path is found, the current working directory is used as well. Theresulting path is normalized, and trailing slashes are removed unless the path gets resolved to the root directory. Non-string arguments are ignored.</p><p>Another way to think of it is as a sequence of <code>cd</code> commands in a shell.</p><pre><code>path.resolve(&#39;foo/bar&#39;, &#39;/tmp/file/&#39;, &#39;..&#39;, &#39;a/../subfile&#39;)</code></pre><p>Is similar to:</p><pre><code>cd foo/barcd /tmp/file/cd ..cd a/../subfilepwd</code></pre><p>The difference is that the different paths don&#39;t need to exist and may also befiles.</p><p>Examples:</p><pre><code>path.resolve(&#39;/foo/bar&#39;, &#39;./baz&#39;)// returns&#39;/foo/bar/baz&#39;path.resolve(&#39;/foo/bar&#39;, &#39;/tmp/file/&#39;)// returns&#39;/tmp/file&#39;path.resolve(&#39;wwwroot&#39;, &#39;static_files/png/&#39;, &#39;../gif/image.gif&#39;)// if currently in /home/myself/node, it returns&#39;/home/myself/node/wwwroot/static_files/gif/image.gif&#39;</code></pre></summary>
        /// <signature>
        /// <param name="from ..."></param>
        /// <param name="to"></param>
        /// </signature>
    }
    this.relative = function(from, to) {
        /// <summary><p>Solve the relative path from <code>from</code> to <code>to</code>.</p><p>At times we have two absolute paths, and we need to derive the relativepath from one to the other.  This is actually the reverse transform of<code>path.resolve</code>, which means we see that:</p><pre><code>path.resolve(from, path.relative(from, to)) == path.resolve(to)</code></pre><p>Examples:</p><pre><code>path.relative(&#39;C:\\orandea\\test\\aaa&#39;, &#39;C:\\orandea\\impl\\bbb&#39;)// returns&#39;..\\..\\impl\\bbb&#39;path.relative(&#39;/data/orandea/test/aaa&#39;, &#39;/data/orandea/impl/bbb&#39;)// returns&#39;../../impl/bbb&#39;</code></pre></summary>
        /// <signature>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// </signature>
    }
    this.dirname = function(p) {
        /// <summary><p>Return the directory name of a path.  Similar to the Unix <code>dirname</code> command.</p><p>Example:</p><pre><code>path.dirname(&#39;/foo/bar/baz/asdf/quux&#39;)// returns&#39;/foo/bar/baz/asdf&#39;</code></pre></summary>
        /// <signature>
        /// <param name="p"></param>
        /// </signature>
    }
    this.basename = function(p, ext) {
        /// <summary><p>Return the last portion of a path.  Similar to the Unix <code>basename</code> command.</p><p>Example:</p><pre><code>path.basename(&#39;/foo/bar/baz/asdf/quux.html&#39;)// returns&#39;quux.html&#39;path.basename(&#39;/foo/bar/baz/asdf/quux.html&#39;, &#39;.html&#39;)// returns&#39;quux&#39;</code></pre></summary>
        /// <signature>
        /// <param name="p"></param>
        /// <param name="ext"></param>
        /// </signature>
    }
    this.extname = function(p) {
        /// <summary><p>Return the extension of the path, from the last &#39;.&#39; to end of stringin the last portion of the path.  If there is no &#39;.&#39; in the last portionof the path or the first character of it is &#39;.&#39;, then it returnsan empty string.  Examples:</p><pre><code>path.extname(&#39;index.html&#39;)// returns&#39;.html&#39;path.extname(&#39;index.&#39;)// returns&#39;.&#39;path.extname(&#39;index&#39;)// returns&#39;&#39;</code></pre></summary>
        /// <signature>
        /// <param name="p"></param>
        /// </signature>
    }
    /// <field name='sep'><p>The platform-specific file separator. <code>&#39;\\&#39;</code> or <code>&#39;/&#39;</code>.</p><p>An example on linux:</p><pre><code>&#39;foo/bar/baz&#39;.split(path.sep)// returns[&#39;foo&#39;, &#39;bar&#39;, &#39;baz&#39;]</code></pre><p>An example on windows:</p><pre><code>&#39;foo\\bar\\baz&#39;.split(path.sep)// returns[&#39;foo&#39;, &#39;bar&#39;, &#39;baz&#39;]</code></pre></field>
    this.sep = undefined;
} // module path

function net() {
    /// <summary><p>The <code>net</code> module provides you with an asynchronous network wrapper. It containsmethods for creating both servers and clients (called streams). You can includethis module with <code>require(&#39;net&#39;);</code></p></summary>
    this.createServer = function(options, connectionListener) {
        /// <summary><p>Creates a new TCP server. The <code>connectionListener</code> argument isautomatically set as a listener for the [&#39;connection&#39;][] event.</p><p><code>options</code> is an object with the following defaults:</p><pre><code>{ allowHalfOpen: false}</code></pre><p>If <code>allowHalfOpen</code> is <code>true</code>, then the socket won&#39;t automatically send a FINpacket when the other end of the socket sends a FIN packet. The socket becomesnon-readable, but still writable. You should call the <code>end()</code> method explicitly.See [&#39;end&#39;][] event for more information.</p><p>Here is an example of an echo server which listens for connectionson port 8124:</p><pre><code>var net = require(&#39;net&#39;);var server = net.createServer(function(c) { //&#39;connection&#39; listener  console.log(&#39;server connected&#39;);  c.on(&#39;end&#39;, function() {    console.log(&#39;server disconnected&#39;);  });  c.write(&#39;hello\r\n&#39;);  c.pipe(c);});server.listen(8124, function() { //&#39;listening&#39; listener  console.log(&#39;server bound&#39;);});</code></pre><p>Test this by using <code>telnet</code>:</p><pre><code>telnet localhost 8124</code></pre><p>To listen on the socket <code>/tmp/echo.sock</code> the third line from the last wouldjust be changed to</p><pre><code>server.listen(&#39;/tmp/echo.sock&#39;, function() { //&#39;listening&#39; listener</code></pre><p>Use <code>nc</code> to connect to a UNIX domain socket server:</p><pre><code>nc -U /tmp/echo.sock</code></pre></summary>
        /// <signature>
        /// <param name="options"></param>
        /// <param name="connectionListener"></param>
        /// </signature>
        return new net.Server();
    }
    this.connect = function(options, connectionListener) {
        /// <summary><p>Constructs a new socket object and opens the socket to the given location.When the socket is established, the [&#39;connect&#39;][] event will be emitted.</p><p>For TCP sockets, <code>options</code> argument should be an object which specifies:</p><ul><li><p><code>port</code>: Port the client should connect to (Required).</p></li><li><p><code>host</code>: Host the client should connect to. Defaults to <code>&#39;localhost&#39;</code>.</p></li><li><p><code>localAddress</code>: Local interface to bind to for network connections.</p></li></ul><p>For UNIX domain sockets, <code>options</code> argument should be an object which specifies:</p><ul><li><code>path</code>: Path the client should connect to (Required).</li></ul><p>Common options are:</p><ul><li><code>allowHalfOpen</code>: if <code>true</code>, the socket won&#39;t automatically senda FIN packet when the other end of the socket sends a FIN packet.Defaults to <code>false</code>.  See [&#39;end&#39;][] event for more information.</li></ul><p>The <code>connectListener</code> parameter will be added as an listener for the[&#39;connect&#39;][] event.</p><p>Here is an example of a client of echo server as described previously:</p><pre><code>var net = require(&#39;net&#39;);var client = net.connect({port: 8124},    function() { //&#39;connect&#39; listener  console.log(&#39;client connected&#39;);  client.write(&#39;world!\r\n&#39;);});client.on(&#39;data&#39;, function(data) {  console.log(data.toString());  client.end();});client.on(&#39;end&#39;, function() {  console.log(&#39;client disconnected&#39;);});</code></pre><p>To connect on the socket <code>/tmp/echo.sock</code> the second line would just bechanged to</p><pre><code>var client = net.connect({path: &#39;/tmp/echo.sock&#39;},</code></pre></summary>
        /// <signature>
        /// <param name="options"></param>
        /// <param name="connectionListener"></param>
        /// </signature>
        /// <signature>
        /// <param name="options"></param>
        /// <param name="connectionListener"></param>
        /// </signature>
    }
    this.createConnection = function(options, connectionListener) {
        /// <summary><p>Constructs a new socket object and opens the socket to the given location.When the socket is established, the [&#39;connect&#39;][] event will be emitted.</p><p>For TCP sockets, <code>options</code> argument should be an object which specifies:</p><ul><li><p><code>port</code>: Port the client should connect to (Required).</p></li><li><p><code>host</code>: Host the client should connect to. Defaults to <code>&#39;localhost&#39;</code>.</p></li><li><p><code>localAddress</code>: Local interface to bind to for network connections.</p></li></ul><p>For UNIX domain sockets, <code>options</code> argument should be an object which specifies:</p><ul><li><code>path</code>: Path the client should connect to (Required).</li></ul><p>Common options are:</p><ul><li><code>allowHalfOpen</code>: if <code>true</code>, the socket won&#39;t automatically senda FIN packet when the other end of the socket sends a FIN packet.Defaults to <code>false</code>.  See [&#39;end&#39;][] event for more information.</li></ul><p>The <code>connectListener</code> parameter will be added as an listener for the[&#39;connect&#39;][] event.</p><p>Here is an example of a client of echo server as described previously:</p><pre><code>var net = require(&#39;net&#39;);var client = net.connect({port: 8124},    function() { //&#39;connect&#39; listener  console.log(&#39;client connected&#39;);  client.write(&#39;world!\r\n&#39;);});client.on(&#39;data&#39;, function(data) {  console.log(data.toString());  client.end();});client.on(&#39;end&#39;, function() {  console.log(&#39;client disconnected&#39;);});</code></pre><p>To connect on the socket <code>/tmp/echo.sock</code> the second line would just bechanged to</p><pre><code>var client = net.connect({path: &#39;/tmp/echo.sock&#39;},</code></pre></summary>
        /// <signature>
        /// <param name="options"></param>
        /// <param name="connectionListener"></param>
        /// </signature>
        return new net.Connection();
    }
    this.connect = function(port, host, connectListener) {
        /// <summary><p>Creates a TCP connection to <code>port</code> on <code>host</code>. If <code>host</code> is omitted,<code>&#39;localhost&#39;</code> will be assumed.The <code>connectListener</code> parameter will be added as an listener for the[&#39;connect&#39;][] event.</p></summary>
        /// <signature>
        /// <param name="port"></param>
        /// <param name="host"></param>
        /// <param name="connectListener"></param>
        /// </signature>
        /// <signature>
        /// <param name="port"></param>
        /// <param name="host"></param>
        /// <param name="connectListener"></param>
        /// </signature>
    }
    this.createConnection = function(port, host, connectListener) {
        /// <summary><p>Creates a TCP connection to <code>port</code> on <code>host</code>. If <code>host</code> is omitted,<code>&#39;localhost&#39;</code> will be assumed.The <code>connectListener</code> parameter will be added as an listener for the[&#39;connect&#39;][] event.</p></summary>
        /// <signature>
        /// <param name="port"></param>
        /// <param name="host"></param>
        /// <param name="connectListener"></param>
        /// </signature>
        return new net.Connection();
    }
    this.connect = function(path, connectListener) {
        /// <summary><p>Creates unix socket connection to <code>path</code>.The <code>connectListener</code> parameter will be added as an listener for the[&#39;connect&#39;][] event.</p></summary>
        /// <signature>
        /// <param name="path"></param>
        /// <param name="connectListener"></param>
        /// </signature>
        /// <signature>
        /// <param name="path"></param>
        /// <param name="connectListener"></param>
        /// </signature>
    }
    this.createConnection = function(path, connectListener) {
        /// <summary><p>Creates unix socket connection to <code>path</code>.The <code>connectListener</code> parameter will be added as an listener for the[&#39;connect&#39;][] event.</p></summary>
        /// <signature>
        /// <param name="path"></param>
        /// <param name="connectListener"></param>
        /// </signature>
        return new net.Connection();
    }
    this.isIP = function(input) {
        /// <summary><p>Tests if input is an IP address. Returns 0 for invalid strings,returns 4 for IP version 4 addresses, and returns 6 for IP version 6 addresses.</p></summary>
        /// <signature>
        /// <param name="input"></param>
        /// </signature>
    }
    this.isIPv4 = function(input) {
        /// <summary><p>Returns true if input is a version 4 IP address, otherwise returns false.</p></summary>
        /// <signature>
        /// <param name="input"></param>
        /// </signature>
    }
    this.isIPv6 = function(input) {
        /// <summary><p>Returns true if input is a version 6 IP address, otherwise returns false.</p></summary>
        /// <signature>
        /// <param name="input"></param>
        /// </signature>
    }
    this.Server = function() {
        this.listen = function(port, host, backlog, callback) {
            /// <summary><p>Begin accepting connections on the specified <code>port</code> and <code>host</code>.  If the<code>host</code> is omitted, the server will accept connections directed to anyIPv4 address (<code>INADDR_ANY</code>). A port value of zero will assign a random port.</p><p>Backlog is the maximum length of the queue of pending connections.The actual length will be determined by your OS through sysctl settings such as<code>tcp_max_syn_backlog</code> and <code>somaxconn</code> on linux. The default value of thisparameter is 511 (not 512).</p><p>This function is asynchronous.  When the server has been bound,[&#39;listening&#39;][] event will be emitted.  The last parameter <code>callback</code>will be added as an listener for the [&#39;listening&#39;][] event.</p><p>One issue some users run into is getting <code>EADDRINUSE</code> errors. This means thatanother server is already running on the requested port. One way of handling thiswould be to wait a second and then try again. This can be done with</p><pre><code>server.on(&#39;error&#39;, function (e) {  if (e.code == &#39;EADDRINUSE&#39;) {    console.log(&#39;Address in use, retrying...&#39;);    setTimeout(function () {      server.close();      server.listen(PORT, HOST);    }, 1000);  }});</code></pre><p>(Note: All sockets in Node set <code>SO_REUSEADDR</code> already)</p></summary>
            /// <signature>
            /// <param name="port"></param>
            /// <param name="host"></param>
            /// <param name="backlog"></param>
            /// <param name="callback"></param>
            /// </signature>
        }
        this.listen = function(path, callback) {
            /// <summary><p>Start a UNIX socket server listening for connections on the given <code>path</code>.</p><p>This function is asynchronous.  When the server has been bound,[&#39;listening&#39;][] event will be emitted.  The last parameter <code>callback</code>will be added as an listener for the [&#39;listening&#39;][] event.</p></summary>
            /// <signature>
            /// <param name="path"></param>
            /// <param name="callback"></param>
            /// </signature>
        }
        this.listen = function(handle, callback) {
            /// <summary><p>The <code>handle</code> object can be set to either a server or socket (anythingwith an underlying <code>_handle</code> member), or a <code>{fd: &lt;n&gt;}</code> object.</p><p>This will cause the server to accept connections on the specifiedhandle, but it is presumed that the file descriptor or handle hasalready been bound to a port or domain socket.</p><p>Listening on a file descriptor is not supported on Windows.</p><p>This function is asynchronous.  When the server has been bound,<a href="#event_listening_">&#39;listening&#39;</a> event will be emitted.the last parameter <code>callback</code> will be added as an listener for the<a href="#event_listening_">&#39;listening&#39;</a> event.</p></summary>
            /// <signature>
            /// <param name="handle" type="Object"></param>
            /// <param name="callback" type="Function"></param>
            /// </signature>
            /// <signature>
            /// <param name="handle"></param>
            /// <param name="callback"></param>
            /// </signature>
        }
        this.close = function(callback) {
            /// <summary><p>Stops the server from accepting new connections and keeps existingconnections. This function is asynchronous, the server is finallyclosed when all connections are ended and the server emits a <code>&#39;close&#39;</code>event. Optionally, you can pass a callback to listen for the <code>&#39;close&#39;</code>event.</p></summary>
            /// <signature>
            /// <param name="callback"></param>
            /// </signature>
        }
        this.address = function() {
            /// <summary><p>Returns the bound address, the address family name and port of the serveras reported by the operating system.Useful to find which port was assigned when giving getting an OS-assigned address.Returns an object with three properties, e.g.<code>{ port: 12346, family: &#39;IPv4&#39;, address: &#39;127.0.0.1&#39; }</code></p><p>Example:</p><pre><code>var server = net.createServer(function (socket) {  socket.end(&quot;goodbye\n&quot;);});// grab a random port.server.listen(function() {  address = server.address();  console.log(&quot;opened server on %j&quot;, address);});</code></pre><p>Don&#39;t call <code>server.address()</code> until the <code>&#39;listening&#39;</code> event has been emitted.</p></summary>
        }
        emitter = new Events().EventEmitter;
        /// <field name='listening'><p>Emitted when the server has been bound after calling <code>server.listen</code>.</p></field>
        this.listening = new emitter();
        /// <field name='connection'><p>Emitted when a new connection is made. <code>socket</code> is an instance of<code>net.Socket</code>.</p></field>
        this.connection = new emitter();
        /// <field name='close'><p>Emitted when the server closes. Note that if connections exist, thisevent is not emitted until all connections are ended.</p></field>
        this.close = new emitter();
        /// <field name='error'><p>Emitted when an error occurs.  The <code>&#39;close&#39;</code> event will be called directlyfollowing this event.  See example in discussion of <code>server.listen</code>.</p></field>
        this.error = new emitter();
        /// <field name='maxConnections'><p>Set this property to reject connections when the server&#39;s connection count getshigh.</p><p>It is not recommended to use this option once a socket has been sent to a childwith <code>child_process.fork()</code>.</p></field>
        this.maxConnections = undefined;
        /// <field name='connections'><p>The number of concurrent connections on the server.</p><p>This becomes <code>null</code> when sending a socket to a child with <code>child_process.fork()</code>.</p><p><code>net.Server</code> is an [EventEmitter][] with the following events:</p></field>
        this.connections = undefined;
    }
    this.Socket = function() {
        this.Socket = function(options) {
            /// <summary><p>Construct a new socket object.</p><p><code>options</code> is an object with the following defaults:</p><pre><code>{ fd: null  type: null  allowHalfOpen: false}</code></pre><p><code>fd</code> allows you to specify the existing file descriptor of socket. <code>type</code>specified underlying protocol. It can be <code>&#39;tcp4&#39;</code>, <code>&#39;tcp6&#39;</code>, or <code>&#39;unix&#39;</code>.About <code>allowHalfOpen</code>, refer to <code>createServer()</code> and <code>&#39;end&#39;</code> event.</p></summary>
            /// <signature>
            /// <param name="options"></param>
            /// </signature>
        }
        this.connect = function(path, connectListener) {
            /// <summary><p>Opens the connection for a given socket. If <code>port</code> and <code>host</code> are given,then the socket will be opened as a TCP socket, if <code>host</code> is omitted,<code>localhost</code> will be assumed. If a <code>path</code> is given, the socket will beopened as a unix socket to that path.</p><p>Normally this method is not needed, as <code>net.createConnection</code> opens thesocket. Use this only if you are implementing a custom Socket or if aSocket is closed and you want to reuse it to connect to another server.</p><p>This function is asynchronous. When the [&#39;connect&#39;][] event is emitted thesocket is established. If there is a problem connecting, the <code>&#39;connect&#39;</code> eventwill not be emitted, the <code>&#39;error&#39;</code> event will be emitted with the exception.</p><p>The <code>connectListener</code> parameter will be added as an listener for the[&#39;connect&#39;][] event.</p></summary>
            /// <signature>
            /// <param name="path"></param>
            /// <param name="connectListener"></param>
            /// </signature>
            /// <signature>
            /// <param name="port"></param>
            /// <param name="host"></param>
            /// <param name="connectListener"></param>
            /// </signature>
        }
        this.connect = function(path, connectListener) {
            /// <summary><p>Opens the connection for a given socket. If <code>port</code> and <code>host</code> are given,then the socket will be opened as a TCP socket, if <code>host</code> is omitted,<code>localhost</code> will be assumed. If a <code>path</code> is given, the socket will beopened as a unix socket to that path.</p><p>Normally this method is not needed, as <code>net.createConnection</code> opens thesocket. Use this only if you are implementing a custom Socket or if aSocket is closed and you want to reuse it to connect to another server.</p><p>This function is asynchronous. When the [&#39;connect&#39;][] event is emitted thesocket is established. If there is a problem connecting, the <code>&#39;connect&#39;</code> eventwill not be emitted, the <code>&#39;error&#39;</code> event will be emitted with the exception.</p><p>The <code>connectListener</code> parameter will be added as an listener for the[&#39;connect&#39;][] event.</p></summary>
            /// <signature>
            /// <param name="path"></param>
            /// <param name="connectListener"></param>
            /// </signature>
        }
        this.setEncoding = function(encoding) {
            /// <summary><p>Set the encoding for the socket as a Readable Stream. See[stream.setEncoding()][] for more information.</p></summary>
            /// <signature>
            /// <param name="encoding"></param>
            /// </signature>
        }
        this.write = function(data, encoding, callback) {
            /// <summary><p>Sends data on the socket. The second parameter specifies the encoding in thecase of a string--it defaults to UTF8 encoding.</p><p>Returns <code>true</code> if the entire data was flushed successfully to the kernelbuffer. Returns <code>false</code> if all or part of the data was queued in user memory.<code>&#39;drain&#39;</code> will be emitted when the buffer is again free.</p><p>The optional <code>callback</code> parameter will be executed when the data is finallywritten out - this may not be immediately.</p></summary>
            /// <signature>
            /// <param name="data"></param>
            /// <param name="encoding"></param>
            /// <param name="callback"></param>
            /// </signature>
        }
        this.end = function(data, encoding) {
            /// <summary><p>Half-closes the socket. i.e., it sends a FIN packet. It is possible theserver will still send some data.</p><p>If <code>data</code> is specified, it is equivalent to calling<code>socket.write(data, encoding)</code> followed by <code>socket.end()</code>.</p></summary>
            /// <signature>
            /// <param name="data"></param>
            /// <param name="encoding"></param>
            /// </signature>
        }
        this.destroy = function() {
            /// <summary><p>Ensures that no more I/O activity happens on this socket. Only necessary incase of errors (parse error or so).</p></summary>
        }
        this.pause = function() {
            /// <summary><p>Pauses the reading of data. That is, <code>&#39;data&#39;</code> events will not be emitted.Useful to throttle back an upload.</p></summary>
        }
        this.resume = function() {
            /// <summary><p>Resumes reading after a call to <code>pause()</code>.</p></summary>
        }
        this.setTimeout = function(timeout, callback) {
            /// <summary><p>Sets the socket to timeout after <code>timeout</code> milliseconds of inactivity onthe socket. By default <code>net.Socket</code> do not have a timeout.</p><p>When an idle timeout is triggered the socket will receive a <code>&#39;timeout&#39;</code>event but the connection will not be severed. The user must manually <code>end()</code>or <code>destroy()</code> the socket.</p><p>If <code>timeout</code> is 0, then the existing idle timeout is disabled.</p><p>The optional <code>callback</code> parameter will be added as a one time listener for the<code>&#39;timeout&#39;</code> event.</p></summary>
            /// <signature>
            /// <param name="timeout"></param>
            /// <param name="callback"></param>
            /// </signature>
        }
        this.setNoDelay = function(noDelay) {
            /// <summary><p>Disables the Nagle algorithm. By default TCP connections use the Naglealgorithm, they buffer data before sending it off. Setting <code>true</code> for<code>noDelay</code> will immediately fire off data each time <code>socket.write()</code> is called.<code>noDelay</code> defaults to <code>true</code>.</p></summary>
            /// <signature>
            /// <param name="noDelay"></param>
            /// </signature>
        }
        this.setKeepAlive = function(enable, initialDelay) {
            /// <summary><p>Enable/disable keep-alive functionality, and optionally set the initialdelay before the first keepalive probe is sent on an idle socket.<code>enable</code> defaults to <code>false</code>.</p><p>Set <code>initialDelay</code> (in milliseconds) to set the delay between the lastdata packet received and the first keepalive probe. Setting 0 forinitialDelay will leave the value unchanged from the default(or previous) setting. Defaults to <code>0</code>.</p></summary>
            /// <signature>
            /// <param name="enable"></param>
            /// <param name="initialDelay"></param>
            /// </signature>
        }
        this.address = function() {
            /// <summary><p>Returns the bound address, the address family name and port of thesocket as reported by the operating system. Returns an object withthree properties, e.g.<code>{ port: 12346, family: &#39;IPv4&#39;, address: &#39;127.0.0.1&#39; }</code></p></summary>
        }
        emitter = new Events().EventEmitter;
        /// <field name='connect'><p>Emitted when a socket connection is successfully established.See <code>connect()</code>.</p></field>
        this.connect = new emitter();
        /// <field name='data'><p>Emitted when data is received.  The argument <code>data</code> will be a <code>Buffer</code> or<code>String</code>.  Encoding of data is set by <code>socket.setEncoding()</code>.(See the [Readable Stream][] section for more information.)</p><p>Note that the <strong>data will be lost</strong> if there is no listener when a <code>Socket</code>emits a <code>&#39;data&#39;</code> event.</p></field>
        this.data = new emitter();
        /// <field name='end'><p>Emitted when the other end of the socket sends a FIN packet.</p><p>By default (<code>allowHalfOpen == false</code>) the socket will destroy its filedescriptor  once it has written out its pending write queue.  However, bysetting <code>allowHalfOpen == true</code> the socket will not automatically <code>end()</code>its side allowing the user to write arbitrary amounts of data, with thecaveat that the user is required to <code>end()</code> their side now.</p></field>
        this.end = new emitter();
        /// <field name='timeout'><p>Emitted if the socket times out from inactivity. This is only to notify thatthe socket has been idle. The user must manually close the connection.</p><p>See also: <code>socket.setTimeout()</code></p></field>
        this.timeout = new emitter();
        /// <field name='drain'><p>Emitted when the write buffer becomes empty. Can be used to throttle uploads.</p><p>See also: the return values of <code>socket.write()</code></p></field>
        this.drain = new emitter();
        /// <field name='error'><p>Emitted when an error occurs.  The <code>&#39;close&#39;</code> event will be called directlyfollowing this event.</p></field>
        this.error = new emitter();
        /// <field name='close'><p>Emitted once the socket is fully closed. The argument <code>had_error</code> is a booleanwhich says if the socket was closed due to a transmission error.</p></field>
        this.close = new emitter();
        /// <field name='bufferSize'><p><code>net.Socket</code> has the property that <code>socket.write()</code> always works. This is tohelp users get up and running quickly. The computer cannot always keep upwith the amount of data that is written to a socket - the network connectionsimply might be too slow. Node will internally queue up the data written to asocket and send it out over the wire when it is possible. (Internally it ispolling on the socket&#39;s file descriptor for being writable).</p><p>The consequence of this internal buffering is that memory may grow. Thisproperty shows the number of characters currently buffered to be written.(Number of characters is approximately equal to the number of bytes to bewritten, but the buffer may contain strings, and the strings are lazilyencoded, so the exact number of bytes is not known.)</p><p>Users who experience large or growing <code>bufferSize</code> should attempt to&quot;throttle&quot; the data flows in their program with <code>pause()</code> and <code>resume()</code>.</p></field>
        this.bufferSize = undefined;
        /// <field name='remoteAddress'><p>The string representation of the remote IP address. For example,<code>&#39;74.125.127.100&#39;</code> or <code>&#39;2001:4860:a005::68&#39;</code>.</p></field>
        this.remoteAddress = undefined;
        /// <field name='remotePort'><p>The numeric representation of the remote port. For example,<code>80</code> or <code>21</code>.</p></field>
        this.remotePort = undefined;
        /// <field name='bytesRead'><p>The amount of received bytes.</p></field>
        this.bytesRead = undefined;
        /// <field name='bytesWritten'><p>The amount of bytes sent.</p><p><code>net.Socket</code> instances are [EventEmitter][] with the following events:</p></field>
        this.bytesWritten = undefined;
    }
} // module net

function dgram() {
    /// <summary><p>Datagram sockets are available through <code>require(&#39;dgram&#39;)</code>.</p></summary>
    this.createSocket = function(type, callback) {
        /// <summary><p>Creates a datagram Socket of the specified types.  Valid types are <code>udp4</code>and <code>udp6</code>.</p><p>Takes an optional callback which is added as a listener for <code>message</code> events.</p><p>Call <code>socket.bind</code> if you want to receive datagrams. <code>socket.bind()</code> will bindto the &quot;all interfaces&quot; address on a random port (it does the right thing forboth <code>udp4</code> and <code>udp6</code> sockets). You can then retrieve the address and portwith <code>socket.address().address</code> and <code>socket.address().port</code>.</p></summary>
        /// <signature>
        /// <param name="type">String. Either 'udp4' or 'udp6'</param>
        /// <param name="callback">Function. Attached as a listener to `message` events.</param>
        /// <returns>Socket object</returns>
        /// </signature>
        /// <signature>
        /// <param name="type"></param>
        /// <param name="callback"></param>
        /// </signature>
        return new dgram.Socket();
    }
    this.Socket = function() {
        this.send = function(buf, offset, length, port, address, callback) {
            /// <summary><p>For UDP sockets, the destination port and IP address must be specified.  A stringmay be supplied for the <code>address</code> parameter, and it will be resolved with DNS.  Anoptional callback may be specified to detect any DNS errors and when <code>buf</code> may bere-used.  Note that DNS lookups will delay the time that a send takes place, atleast until the next tick.  The only way to know for sure that a send has taken placeis to use the callback.</p><p>If the socket has not been previously bound with a call to <code>bind</code>, it&#39;sassigned a random port number and bound to the &quot;all interfaces&quot; address(0.0.0.0 for <code>udp4</code> sockets, ::0 for <code>udp6</code> sockets).</p><p>Example of sending a UDP packet to a random port on <code>localhost</code>;</p><pre><code>var dgram = require(&#39;dgram&#39;);var message = new Buffer(&quot;Some bytes&quot;);var client = dgram.createSocket(&quot;udp4&quot;);client.send(message, 0, message.length, 41234, &quot;localhost&quot;, function(err, bytes) {  client.close();});</code></pre><p><strong>A Note about UDP datagram size</strong></p><p>The maximum size of an <code>IPv4/v6</code> datagram depends on the <code>MTU</code> (<em>Maximum Transmission Unit</em>)and on the <code>Payload Length</code> field size.</p><ul><li><p>The <code>Payload Length</code> field is <code>16 bits</code> wide, which means that a normal payloadcannot be larger than 64K octets including internet header and data(65,507 bytes = 65,535 âˆ’ 8 bytes UDP header âˆ’ 20 bytes IP header);this is generally true for loopback interfaces, but such long datagramsare impractical for most hosts and networks.</p></li><li><p>The <code>MTU</code> is the largest size a given link layer technology can support for datagrams.For any link, <code>IPv4</code> mandates a minimum <code>MTU</code> of <code>68</code> octets, while the recommended <code>MTU</code>for IPv4 is <code>576</code> (typically recommended as the <code>MTU</code> for dial-up type applications),whether they arrive whole or in fragments.</p><p>For <code>IPv6</code>, the minimum <code>MTU</code> is <code>1280</code> octets, however, the mandatory minimumfragment reassembly buffer size is <code>1500</code> octets.The value of <code>68</code> octets is very small, since most current link layer technologies havea minimum <code>MTU</code> of <code>1500</code> (like Ethernet).</p></li></ul><p>Note that it&#39;s impossible to know in advance the MTU of each link through whicha packet might travel, and that generally sending a datagram greater thanthe (receiver) <code>MTU</code> won&#39;t work (the packet gets silently dropped, withoutinforming the source that the data did not reach its intended recipient).</p></summary>
            /// <signature>
            /// <param name="buf">Buffer object.  Message to be sent</param>
            /// <param name="offset">Integer. Offset in the buffer where the message starts.</param>
            /// <param name="length">Integer. Number of bytes in the message.</param>
            /// <param name="port">Integer. destination port</param>
            /// <param name="address">String. destination IP</param>
            /// <param name="callback">Function. Callback when message is done being delivered. Optional.</param>
            /// </signature>
            /// <signature>
            /// <param name="buf"></param>
            /// <param name="offset"></param>
            /// <param name="length"></param>
            /// <param name="port"></param>
            /// <param name="address"></param>
            /// <param name="callback"></param>
            /// </signature>
        }
        this.bind = function(port, address) {
            /// <summary><p>For UDP sockets, listen for datagrams on a named <code>port</code> and optional <code>address</code>. If<code>address</code> is not specified, the OS will try to listen on all addresses.</p><p>Example of a UDP server listening on port 41234:</p><pre><code>var dgram = require(&quot;dgram&quot;);var server = dgram.createSocket(&quot;udp4&quot;);server.on(&quot;message&quot;, function (msg, rinfo) {  console.log(&quot;server got: &quot; + msg + &quot; from &quot; +    rinfo.address + &quot;:&quot; + rinfo.port);});server.on(&quot;listening&quot;, function () {  var address = server.address();  console.log(&quot;server listening &quot; +      address.address + &quot;:&quot; + address.port);});server.bind(41234);// server listening 0.0.0.0:41234</code></pre></summary>
            /// <signature>
            /// <param name="port">Integer</param>
            /// <param name="address">String</param>
            /// </signature>
            /// <signature>
            /// <param name="port"></param>
            /// <param name="address"></param>
            /// </signature>
        }
        this.close = function() {
            /// <summary><p>Close the underlying socket and stop listening for data on it.</p></summary>
        }
        this.address = function() {
            /// <summary><p>Returns an object containing the address information for a socket.  For UDP sockets,this object will contain <code>address</code> , <code>family</code> and <code>port</code>.</p></summary>
        }
        this.setBroadcast = function(flag) {
            /// <summary><p>Sets or clears the <code>SO_BROADCAST</code> socket option.  When this option is set, UDP packetsmay be sent to a local interface&#39;s broadcast address.</p></summary>
            /// <signature>
            /// <param name="flag">Boolean</param>
            /// </signature>
            /// <signature>
            /// <param name="flag"></param>
            /// </signature>
        }
        this.setTTL = function(ttl) {
            /// <summary><p>Sets the <code>IP_TTL</code> socket option.  TTL stands for &quot;Time to Live,&quot; but in this context itspecifies the number of IP hops that a packet is allowed to go through.  Each router orgateway that forwards a packet decrements the TTL.  If the TTL is decremented to 0 by arouter, it will not be forwarded.  Changing TTL values is typically done for networkprobes or when multicasting.</p><p>The argument to <code>setTTL()</code> is a number of hops between 1 and 255.  The default on mostsystems is 64.</p></summary>
            /// <signature>
            /// <param name="ttl">Integer</param>
            /// </signature>
            /// <signature>
            /// <param name="ttl"></param>
            /// </signature>
        }
        this.setMulticastTTL = function(ttl) {
            /// <summary><p>Sets the <code>IP_MULTICAST_TTL</code> socket option.  TTL stands for &quot;Time to Live,&quot; but in thiscontext it specifies the number of IP hops that a packet is allowed to go through,specifically for multicast traffic.  Each router or gateway that forwards a packetdecrements the TTL. If the TTL is decremented to 0 by a router, it will not be forwarded.</p><p>The argument to <code>setMulticastTTL()</code> is a number of hops between 0 and 255.  The default on mostsystems is 1.</p></summary>
            /// <signature>
            /// <param name="ttl">Integer</param>
            /// </signature>
            /// <signature>
            /// <param name="ttl"></param>
            /// </signature>
        }
        this.setMulticastLoopback = function(flag) {
            /// <summary><p>Sets or clears the <code>IP_MULTICAST_LOOP</code> socket option.  When this option is set, multicastpackets will also be received on the local interface.</p></summary>
            /// <signature>
            /// <param name="flag">Boolean</param>
            /// </signature>
            /// <signature>
            /// <param name="flag"></param>
            /// </signature>
        }
        this.addMembership = function(multicastAddress, multicastInterface) {
            /// <summary><p>Tells the kernel to join a multicast group with <code>IP_ADD_MEMBERSHIP</code> socket option.</p><p>If <code>multicastInterface</code> is not specified, the OS will try to add membership to all validinterfaces.</p></summary>
            /// <signature>
            /// <param name="multicastAddress">String</param>
            /// <param name="multicastInterface">String</param>
            /// </signature>
            /// <signature>
            /// <param name="multicastAddress"></param>
            /// <param name="multicastInterface"></param>
            /// </signature>
        }
        this.dropMembership = function(multicastAddress, multicastInterface) {
            /// <summary><p>Opposite of <code>addMembership</code> - tells the kernel to leave a multicast group with<code>IP_DROP_MEMBERSHIP</code> socket option. This is automatically called by the kernelwhen the socket is closed or process terminates, so most apps will never need to callthis.</p><p>If <code>multicastInterface</code> is not specified, the OS will try to drop membership to all validinterfaces.</p></summary>
            /// <signature>
            /// <param name="multicastAddress">String</param>
            /// <param name="multicastInterface">String</param>
            /// </signature>
            /// <signature>
            /// <param name="multicastAddress"></param>
            /// <param name="multicastInterface"></param>
            /// </signature>
        }
        emitter = new Events().EventEmitter;
        /// <field name='message'><p>Emitted when a new datagram is available on a socket.  <code>msg</code> is a <code>Buffer</code> and <code>rinfo</code> isan object with the sender&#39;s address information and the number of bytes in the datagram.</p></field>
        this.message = new emitter();
        /// <field name='listening'><p>Emitted when a socket starts listening for datagrams.  This happens as soon as UDP socketsare created.</p></field>
        this.listening = new emitter();
        /// <field name='close'><p>Emitted when a socket is closed with <code>close()</code>.  No new <code>message</code> events will be emittedon this socket.</p></field>
        this.close = new emitter();
        /// <field name='error'><p>Emitted when an error occurs.</p></field>
        this.error = new emitter();
    }
} // module dgram

function dns() {
    /// <summary><p>Use <code>require(&#39;dns&#39;)</code> to access this module. All methods in the dns moduleuse C-Ares except for <code>dns.lookup</code> which uses <code>getaddrinfo(3)</code> in a threadpool. C-Ares is much faster than <code>getaddrinfo</code> but the system resolver ismore constant with how other programs operate. When a user does<code>net.connect(80, &#39;google.com&#39;)</code> or <code>http.get({ host: &#39;google.com&#39; })</code> the<code>dns.lookup</code> method is used. Users who need to do a large number of look upsquickly should use the methods that go through C-Ares.</p><p>Here is an example which resolves <code>&#39;www.google.com&#39;</code> then reverseresolves the IP addresses which are returned.</p><pre><code>var dns = require(&#39;dns&#39;);dns.resolve4(&#39;www.google.com&#39;, function (err, addresses) {  if (err) throw err;  console.log(&#39;addresses: &#39; + JSON.stringify(addresses));  addresses.forEach(function (a) {    dns.reverse(a, function (err, domains) {      if (err) {        throw err;      }      console.log(&#39;reverse for &#39; + a + &#39;: &#39; + JSON.stringify(domains));    });  });});</code></pre></summary>
    this.lookup = function(domain, family, callback) {
        /// <summary><p>Resolves a domain (e.g. <code>&#39;google.com&#39;</code>) into the first found A (IPv4) orAAAA (IPv6) record.The <code>family</code> can be the integer <code>4</code> or <code>6</code>. Defaults to <code>null</code> that indicatesboth Ip v4 and v6 address family.</p><p>The callback has arguments <code>(err, address, family)</code>.  The <code>address</code> argumentis a string representation of a IP v4 or v6 address. The <code>family</code> argumentis either the integer 4 or 6 and denotes the family of <code>address</code> (notnecessarily the value initially passed to <code>lookup</code>).</p><p>On error, <code>err</code> is an <code>Error</code> object, where <code>err.code</code> is the error code.Keep in mind that <code>err.code</code> will be set to <code>&#39;ENOENT&#39;</code> not only whenthe domain does not exist but also when the lookup fails in other wayssuch as no available file descriptors.</p></summary>
        /// <signature>
        /// <param name="domain"></param>
        /// <param name="family"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.resolve = function(domain, rrtype, callback) {
        /// <summary><p>Resolves a domain (e.g. <code>&#39;google.com&#39;</code>) into an array of the record typesspecified by rrtype. Valid rrtypes are <code>&#39;A&#39;</code> (IPV4 addresses, default),<code>&#39;AAAA&#39;</code> (IPV6 addresses), <code>&#39;MX&#39;</code> (mail exchange records), <code>&#39;TXT&#39;</code> (textrecords), <code>&#39;SRV&#39;</code> (SRV records), <code>&#39;PTR&#39;</code> (used for reverse IP lookups),<code>&#39;NS&#39;</code> (name server records) and <code>&#39;CNAME&#39;</code> (canonical name records).</p><p>The callback has arguments <code>(err, addresses)</code>.  The type of each itemin <code>addresses</code> is determined by the record type, and described in thedocumentation for the corresponding lookup methods below.</p><p>On error, <code>err</code> is an <code>Error</code> object, where <code>err.code</code> isone of the error codes listed below.</p></summary>
        /// <signature>
        /// <param name="domain"></param>
        /// <param name="rrtype"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.resolve4 = function(domain, callback) {
        /// <summary><p>The same as <code>dns.resolve()</code>, but only for IPv4 queries (<code>A</code> records).<code>addresses</code> is an array of IPv4 addresses (e.g.<code>[&#39;74.125.79.104&#39;, &#39;74.125.79.105&#39;, &#39;74.125.79.106&#39;]</code>).</p></summary>
        /// <signature>
        /// <param name="domain"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.resolve6 = function(domain, callback) {
        /// <summary><p>The same as <code>dns.resolve4()</code> except for IPv6 queries (an <code>AAAA</code> query).</p></summary>
        /// <signature>
        /// <param name="domain"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.resolveMx = function(domain, callback) {
        /// <summary><p>The same as <code>dns.resolve()</code>, but only for mail exchange queries (<code>MX</code> records).</p><p><code>addresses</code> is an array of MX records, each with a priority and an exchangeattribute (e.g. <code>[{&#39;priority&#39;: 10, &#39;exchange&#39;: &#39;mx.example.com&#39;},...]</code>).</p></summary>
        /// <signature>
        /// <param name="domain"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.resolveTxt = function(domain, callback) {
        /// <summary><p>The same as <code>dns.resolve()</code>, but only for text queries (<code>TXT</code> records).<code>addresses</code> is an array of the text records available for <code>domain</code> (e.g.,<code>[&#39;v=spf1 ip4:0.0.0.0 ~all&#39;]</code>).</p></summary>
        /// <signature>
        /// <param name="domain"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.resolveSrv = function(domain, callback) {
        /// <summary><p>The same as <code>dns.resolve()</code>, but only for service records (<code>SRV</code> records).<code>addresses</code> is an array of the SRV records available for <code>domain</code>. Propertiesof SRV records are priority, weight, port, and name (e.g.,<code>[{&#39;priority&#39;: 10, {&#39;weight&#39;: 5, &#39;port&#39;: 21223, &#39;name&#39;: &#39;service.example.com&#39;}, ...]</code>).</p></summary>
        /// <signature>
        /// <param name="domain"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.resolveNs = function(domain, callback) {
        /// <summary><p>The same as <code>dns.resolve()</code>, but only for name server records (<code>NS</code> records).<code>addresses</code> is an array of the name server records available for <code>domain</code>(e.g., <code>[&#39;ns1.example.com&#39;, &#39;ns2.example.com&#39;]</code>).</p></summary>
        /// <signature>
        /// <param name="domain"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.resolveCname = function(domain, callback) {
        /// <summary><p>The same as <code>dns.resolve()</code>, but only for canonical name records (<code>CNAME</code>records). <code>addresses</code> is an array of the canonical name records available for<code>domain</code> (e.g., <code>[&#39;bar.example.com&#39;]</code>).</p></summary>
        /// <signature>
        /// <param name="domain"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.reverse = function(ip, callback) {
        /// <summary><p>Reverse resolves an ip address to an array of domain names.</p><p>The callback has arguments <code>(err, domains)</code>.</p><p>On error, <code>err</code> is an <code>Error</code> object, where <code>err.code</code> isone of the error codes listed below.</p></summary>
        /// <signature>
        /// <param name="ip"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
} // module dns

function http() {
    /// <summary><p>To use the HTTP server and client one must <code>require(&#39;http&#39;)</code>.</p><p>The HTTP interfaces in Node are designed to support many featuresof the protocol which have been traditionally difficult to use.In particular, large, possibly chunk-encoded, messages. The interface iscareful to never buffer entire requests or responses--theuser is able to stream data.</p><p>HTTP message headers are represented by an object like this:</p><pre><code>{ &#39;content-length&#39;: &#39;123&#39;,  &#39;content-type&#39;: &#39;text/plain&#39;,  &#39;connection&#39;: &#39;keep-alive&#39;,  &#39;accept&#39;: &#39;*/*&#39; }</code></pre><p>Keys are lowercased. Values are not modified.</p><p>In order to support the full spectrum of possible HTTP applications, Node&#39;sHTTP API is very low-level. It deals with stream handling and messageparsing only. It parses a message into headers and body but it does notparse the actual headers or the body.</p></summary>
    this.createServer = function(requestListener) {
        /// <summary><p>Returns a new web server object.</p><p>The <code>requestListener</code> is a function which is automaticallyadded to the <code>&#39;request&#39;</code> event.</p></summary>
        /// <signature>
        /// <param name="requestListener"></param>
        /// </signature>
        return new http.Server();
    }
    this.createClient = function(port, host) {
        /// <summary><p>This function is <strong>deprecated</strong>; please use [http.request()][] instead.Constructs a new HTTP client. <code>port</code> and <code>host</code> refer to the server to beconnected to.</p></summary>
        /// <signature>
        /// <param name="port"></param>
        /// <param name="host"></param>
        /// </signature>
        return new http.Client();
    }
    this.request = function(options, callback) {
        /// <summary><p>Node maintains several connections per server to make HTTP requests.This function allows one to transparently issue requests.</p><p><code>options</code> can be an object or a string. If <code>options</code> is a string, it isautomatically parsed with [url.parse()][].</p><p>Options:</p><ul><li><code>host</code>: A domain name or IP address of the server to issue the request to.Defaults to <code>&#39;localhost&#39;</code>.</li><li><code>hostname</code>: To support <code>url.parse()</code> <code>hostname</code> is preferred over <code>host</code></li><li><code>port</code>: Port of remote server. Defaults to 80.</li><li><code>localAddress</code>: Local interface to bind for network connections.</li><li><code>socketPath</code>: Unix Domain Socket (use one of host:port or socketPath)</li><li><code>method</code>: A string specifying the HTTP request method. Defaults to <code>&#39;GET&#39;</code>.</li><li><code>path</code>: Request path. Defaults to <code>&#39;/&#39;</code>. Should include query string if any.E.G. <code>&#39;/index.html?page=12&#39;</code></li><li><code>headers</code>: An object containing request headers.</li><li><code>auth</code>: Basic authentication i.e. <code>&#39;user:password&#39;</code> to compute anAuthorization header.</li><li><code>agent</code>: Controls [Agent][] behavior. When an Agent is used request willdefault to <code>Connection: keep-alive</code>. Possible values:<ul><li><code>undefined</code> (default): use [global Agent][] for this host and port.</li><li><code>Agent</code> object: explicitly use the passed in <code>Agent</code>.</li><li><code>false</code>: opts out of connection pooling with an Agent, defaults request to<code>Connection: close</code>.</li></ul></li></ul><p><code>http.request()</code> returns an instance of the <code>http.ClientRequest</code>class. The <code>ClientRequest</code> instance is a writable stream. If one needs toupload a file with a POST request, then write to the <code>ClientRequest</code> object.</p><p>Example:</p><pre><code>var options = {  host: &#39;www.google.com&#39;,  port: 80,  path: &#39;/upload&#39;,  method: &#39;POST&#39;};var req = http.request(options, function(res) {  console.log(&#39;STATUS: &#39; + res.statusCode);  console.log(&#39;HEADERS: &#39; + JSON.stringify(res.headers));  res.setEncoding(&#39;utf8&#39;);  res.on(&#39;data&#39;, function (chunk) {    console.log(&#39;BODY: &#39; + chunk);  });});req.on(&#39;error&#39;, function(e) {  console.log(&#39;problem with request: &#39; + e.message);});// write data to request bodyreq.write(&#39;data\n&#39;);req.write(&#39;data\n&#39;);req.end();</code></pre><p>Note that in the example <code>req.end()</code> was called. With <code>http.request()</code> onemust always call <code>req.end()</code> to signify that you&#39;re done with the request -even if there is no data being written to the request body.</p><p>If any error is encountered during the request (be that with DNS resolution,TCP level errors, or actual HTTP parse errors) an <code>&#39;error&#39;</code> event is emittedon the returned request object.</p><p>There are a few special headers that should be noted.</p><ul><li><p>Sending a &#39;Connection: keep-alive&#39; will notify Node that the connection tothe server should be persisted until the next request.</p></li><li><p>Sending a &#39;Content-length&#39; header will disable the default chunked encoding.</p></li><li><p>Sending an &#39;Expect&#39; header will immediately send the request headers.Usually, when sending &#39;Expect: 100-continue&#39;, you should both set a timeoutand listen for the <code>continue</code> event. See RFC2616 Section 8.2.3 for moreinformation.</p></li><li><p>Sending an Authorization header will override using the <code>auth</code> optionto compute basic authentication.</p></li></ul></summary>
        /// <signature>
        /// <param name="options"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.get = function(options, callback) {
        /// <summary><p>Since most requests are GET requests without bodies, Node provides thisconvenience method. The only difference between this method and <code>http.request()</code>is that it sets the method to GET and calls <code>req.end()</code> automatically.</p><p>Example:</p><pre><code>http.get(&quot;http://www.google.com/index.html&quot;, function(res) {  console.log(&quot;Got response: &quot; + res.statusCode);}).on(&#39;error&#39;, function(e) {  console.log(&quot;Got error: &quot; + e.message);});</code></pre></summary>
        /// <signature>
        /// <param name="options"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.Server = function() {
        this.listen = function(port, hostname, backlog, callback) {
            /// <summary><p>Begin accepting connections on the specified port and hostname.  If thehostname is omitted, the server will accept connections directed to anyIPv4 address (<code>INADDR_ANY</code>).</p><p>To listen to a unix socket, supply a filename instead of port and hostname.</p><p>Backlog is the maximum length of the queue of pending connections.The actual length will be determined by your OS through sysctl settings such as<code>tcp_max_syn_backlog</code> and <code>somaxconn</code> on linux. The default value of thisparameter is 511 (not 512).</p><p>This function is asynchronous. The last parameter <code>callback</code> will be added asa listener for the [&#39;listening&#39;][] event.  See also [net.Server.listen(port)][].</p></summary>
            /// <signature>
            /// <param name="port"></param>
            /// <param name="hostname"></param>
            /// <param name="backlog"></param>
            /// <param name="callback"></param>
            /// </signature>
        }
        this.listen = function(path, callback) {
            /// <summary><p>Start a UNIX socket server listening for connections on the given <code>path</code>.</p><p>This function is asynchronous. The last parameter <code>callback</code> will be added asa listener for the [&#39;listening&#39;][] event.  See also [net.Server.listen(path)][].</p></summary>
            /// <signature>
            /// <param name="path"></param>
            /// <param name="callback"></param>
            /// </signature>
        }
        this.listen = function(handle, callback) {
            /// <summary><p>The <code>handle</code> object can be set to either a server or socket (anythingwith an underlying <code>_handle</code> member), or a <code>{fd: &lt;n&gt;}</code> object.</p><p>This will cause the server to accept connections on the specifiedhandle, but it is presumed that the file descriptor or handle hasalready been bound to a port or domain socket.</p><p>Listening on a file descriptor is not supported on Windows.</p><p>This function is asynchronous. The last parameter <code>callback</code> will be added asa listener for the <a href="net.html#event_listening_">&#39;listening&#39;</a> event.See also <a href="net.html#net_server_listen_handle_callback">net.Server.listen()</a>.</p></summary>
            /// <signature>
            /// <param name="handle" type="Object"></param>
            /// <param name="callback" type="Function"></param>
            /// </signature>
            /// <signature>
            /// <param name="handle"></param>
            /// <param name="callback"></param>
            /// </signature>
        }
        this.close = function(callback) {
            /// <summary><p>Stops the server from accepting new connections.  See [net.Server.close()][].</p></summary>
            /// <signature>
            /// <param name="callback"></param>
            /// </signature>
        }
        emitter = new Events().EventEmitter;
        /// <field name='request'><p><code>function (request, response) { }</code></p><p>Emitted each time there is a request. Note that there may be multiple requestsper connection (in the case of keep-alive connections). <code>request</code> is an instance of <code>http.ServerRequest</code> and <code>response</code> is an instance of <code>http.ServerResponse</code></p></field>
        this.request = new emitter();
        /// <field name='connection'><p><code>function (socket) { }</code></p><p> When a new TCP stream is established. <code>socket</code> is an object of type <code>net.Socket</code>. Usually users will not want to access this event. The <code>socket</code> can also be accessed at <code>request.connection</code>.</p></field>
        this.connection = new emitter();
        /// <field name='close'><p><code>function () { }</code></p><p> Emitted when the server closes.</p></field>
        this.close = new emitter();
        /// <field name='checkContinue'><p><code>function (request, response) { }</code></p><p>Emitted each time a request with an http Expect: 100-continue is received.If this event isn&#39;t listened for, the server will automatically respondwith a 100 Continue as appropriate.</p><p>Handling this event involves calling <code>response.writeContinue</code> if the clientshould continue to send the request body, or generating an appropriate HTTPresponse (e.g., 400 Bad Request) if the client should not continue to send therequest body.</p><p>Note that when this event is emitted and handled, the <code>request</code> event willnot be emitted.</p></field>
        this.checkContinue = new emitter();
        /// <field name='connect'><p><code>function (request, socket, head) { }</code></p><p>Emitted each time a client requests a http CONNECT method. If this event isn&#39;tlistened for, then clients requesting a CONNECT method will have theirconnections closed.</p><ul><li><code>request</code> is the arguments for the http request, as it is in the requestevent.</li><li><code>socket</code> is the network socket between the server and client.</li><li><code>head</code> is an instance of Buffer, the first packet of the tunneling stream,this may be empty.</li></ul><p>After this event is emitted, the request&#39;s socket will not have a <code>data</code>event listener, meaning you will need to bind to it in order to handle datasent to the server on that socket.</p></field>
        this.connect = new emitter();
        /// <field name='upgrade'><p><code>function (request, socket, head) { }</code></p><p>Emitted each time a client requests a http upgrade. If this event isn&#39;tlistened for, then clients requesting an upgrade will have their connectionsclosed.</p><ul><li><code>request</code> is the arguments for the http request, as it is in the requestevent.</li><li><code>socket</code> is the network socket between the server and client.</li><li><code>head</code> is an instance of Buffer, the first packet of the upgraded stream,this may be empty.</li></ul><p>After this event is emitted, the request&#39;s socket will not have a <code>data</code>event listener, meaning you will need to bind to it in order to handle datasent to the server on that socket.</p></field>
        this.upgrade = new emitter();
        /// <field name='clientError'><p><code>function (exception) { }</code></p><p>If a client connection emits an &#39;error&#39; event - it will forwarded here.</p></field>
        this.clientError = new emitter();
        /// <field name='maxHeadersCount'><p>Limits maximum incoming headers count, equal to 1000 by default. If set to 0 -no limit will be applied.</p></field>
        this.maxHeadersCount = undefined;
    }
    this.ServerRequest = function() {
        this.setEncoding = function(encoding) {
            /// <summary><p>Set the encoding for the request body. See [stream.setEncoding()][] for moreinformation.</p></summary>
            /// <signature>
            /// <param name="encoding"></param>
            /// </signature>
        }
        this.pause = function() {
            /// <summary><p>Pauses request from emitting events.  Useful to throttle back an upload.</p></summary>
        }
        this.resume = function() {
            /// <summary><p>Resumes a paused request.</p></summary>
        }
        emitter = new Events().EventEmitter;
        /// <field name='data'><p><code>function (chunk) { }</code></p><p>Emitted when a piece of the message body is received. The chunk is a string ifan encoding has been set with <code>request.setEncoding()</code>, otherwise it&#39;s a[Buffer][].</p><p>Note that the <strong>data will be lost</strong> if there is no listener when a<code>ServerRequest</code> emits a <code>&#39;data&#39;</code> event.</p></field>
        this.data = new emitter();
        /// <field name='end'><p><code>function () { }</code></p><p>Emitted exactly once for each request. After that, no more <code>&#39;data&#39;</code> eventswill be emitted on the request.</p></field>
        this.end = new emitter();
        /// <field name='close'><p><code>function () { }</code></p><p>Indicates that the underlaying connection was terminated before<code>response.end()</code> was called or able to flush.</p><p>Just like <code>&#39;end&#39;</code>, this event occurs only once per request, and no more <code>&#39;data&#39;</code>events will fire afterwards.</p><p>Note: <code>&#39;close&#39;</code> can fire after <code>&#39;end&#39;</code>, but not vice versa.</p></field>
        this.close = new emitter();
        /// <field name='method'><p>The request method as a string. Read only. Example:<code>&#39;GET&#39;</code>, <code>&#39;DELETE&#39;</code>.</p></field>
        this.method = undefined;
        /// <field name='url'><p>Request URL string. This contains only the URL that ispresent in the actual HTTP request. If the request is:</p><pre><code>GET /status?name=ryan HTTP/1.1\r\nAccept: text/plain\r\n\r\n</code></pre><p>Then <code>request.url</code> will be:</p><pre><code>&#39;/status?name=ryan&#39;</code></pre><p>If you would like to parse the URL into its parts, you can use<code>require(&#39;url&#39;).parse(request.url)</code>.  Example:</p><pre><code>node&gt; require(&#39;url&#39;).parse(&#39;/status?name=ryan&#39;){ href: &#39;/status?name=ryan&#39;,  search: &#39;?name=ryan&#39;,  query: &#39;name=ryan&#39;,  pathname: &#39;/status&#39; }</code></pre><p>If you would like to extract the params from the query string,you can use the <code>require(&#39;querystring&#39;).parse</code> function, or pass<code>true</code> as the second argument to <code>require(&#39;url&#39;).parse</code>.  Example:</p><pre><code>node&gt; require(&#39;url&#39;).parse(&#39;/status?name=ryan&#39;, true){ href: &#39;/status?name=ryan&#39;,  search: &#39;?name=ryan&#39;,  query: { name: &#39;ryan&#39; },  pathname: &#39;/status&#39; }</code></pre></field>
        this.url = undefined;
        /// <field name='headers'><p>Read only map of header names and values. Header names are lower-cased.Example:</p><pre><code>// Prints something like://// { &#39;user-agent&#39;: &#39;curl/7.22.0&#39;,//   host: &#39;127.0.0.1:8000&#39;,//   accept: &#39;*/*&#39; }console.log(request.headers);</code></pre></field>
        this.headers = undefined;
        /// <field name='trailers'><p>Read only; HTTP trailers (if present). Only populated after the &#39;end&#39; event.</p></field>
        this.trailers = undefined;
        /// <field name='httpVersion'><p>The HTTP protocol version as a string. Read only. Examples:<code>&#39;1.1&#39;</code>, <code>&#39;1.0&#39;</code>.Also <code>request.httpVersionMajor</code> is the first integer and<code>request.httpVersionMinor</code> is the second.</p></field>
        this.httpVersion = undefined;
        /// <field name='connection'><p>The <code>net.Socket</code> object associated with the connection.</p><p>With HTTPS support, use request.connection.verifyPeer() andrequest.connection.getPeerCertificate() to obtain the client&#39;sauthentication details.</p></field>
        this.connection = undefined;
    }
    this.ServerResponse = function() {
        this.writeContinue = function() {
            /// <summary><p>Sends a HTTP/1.1 100 Continue message to the client, indicating thatthe request body should be sent. See the [&#39;checkContinue&#39;][] event on <code>Server</code>.</p></summary>
        }
        this.writeHead = function(statusCode, reasonPhrase, headers) {
            /// <summary><p>Sends a response header to the request. The status code is a 3-digit HTTPstatus code, like <code>404</code>. The last argument, <code>headers</code>, are the response headers.Optionally one can give a human-readable <code>reasonPhrase</code> as the secondargument.</p><p>Example:</p><pre><code>var body = &#39;hello world&#39;;response.writeHead(200, {  &#39;Content-Length&#39;: body.length,  &#39;Content-Type&#39;: &#39;text/plain&#39; });</code></pre><p>This method must only be called once on a message and it mustbe called before <code>response.end()</code> is called.</p><p>If you call <code>response.write()</code> or <code>response.end()</code> before calling this, theimplicit/mutable headers will be calculated and call this function for you.</p><p>Note: that Content-Length is given in bytes not characters. The above exampleworks because the string <code>&#39;hello world&#39;</code> contains only single byte characters.If the body contains higher coded characters then <code>Buffer.byteLength()</code>should be used to determine the number of bytes in a given encoding.And Node does not check whether Content-Length and the length of the bodywhich has been transmitted are equal or not.</p></summary>
            /// <signature>
            /// <param name="statusCode"></param>
            /// <param name="reasonPhrase"></param>
            /// <param name="headers"></param>
            /// </signature>
        }
        this.setHeader = function(name, value) {
            /// <summary><p>Sets a single header value for implicit headers.  If this header already existsin the to-be-sent headers, its value will be replaced.  Use an array of stringshere if you need to send multiple headers with the same name.</p><p>Example:</p><pre><code>response.setHeader(&quot;Content-Type&quot;, &quot;text/html&quot;);</code></pre><p>or</p><pre><code>response.setHeader(&quot;Set-Cookie&quot;, [&quot;type=ninja&quot;, &quot;language=javascript&quot;]);</code></pre></summary>
            /// <signature>
            /// <param name="name"></param>
            /// <param name="value"></param>
            /// </signature>
        }
        this.getHeader = function(name) {
            /// <summary><p>Reads out a header that&#39;s already been queued but not sent to the client.  Notethat the name is case insensitive.  This can only be called before headers getimplicitly flushed.</p><p>Example:</p><pre><code>var contentType = response.getHeader(&#39;content-type&#39;);</code></pre></summary>
            /// <signature>
            /// <param name="name"></param>
            /// </signature>
        }
        this.removeHeader = function(name) {
            /// <summary><p>Removes a header that&#39;s queued for implicit sending.</p><p>Example:</p><pre><code>response.removeHeader(&quot;Content-Encoding&quot;);</code></pre></summary>
            /// <signature>
            /// <param name="name"></param>
            /// </signature>
        }
        this.write = function(chunk, encoding) {
            /// <summary><p>If this method is called and <code>response.writeHead()</code> has not been called, it willswitch to implicit header mode and flush the implicit headers.</p><p>This sends a chunk of the response body. This method maybe called multiple times to provide successive parts of the body.</p><p><code>chunk</code> can be a string or a buffer. If <code>chunk</code> is a string,the second parameter specifies how to encode it into a byte stream.By default the <code>encoding</code> is <code>&#39;utf8&#39;</code>.</p><p><strong>Note</strong>: This is the raw HTTP body and has nothing to do withhigher-level multi-part body encodings that may be used.</p><p>The first time <code>response.write()</code> is called, it will send the bufferedheader information and the first body to the client. The second time<code>response.write()</code> is called, Node assumes you&#39;re going to be streamingdata, and sends that separately. That is, the response is buffered up to thefirst chunk of body.</p><p>Returns <code>true</code> if the entire data was flushed successfully to the kernelbuffer. Returns <code>false</code> if all or part of the data was queued in user memory.<code>&#39;drain&#39;</code> will be emitted when the buffer is again free.</p></summary>
            /// <signature>
            /// <param name="chunk"></param>
            /// <param name="encoding"></param>
            /// </signature>
        }
        this.addTrailers = function(headers) {
            /// <summary><p>This method adds HTTP trailing headers (a header but at the end of themessage) to the response.</p><p>Trailers will <strong>only</strong> be emitted if chunked encoding is used for theresponse; if it is not (e.g., if the request was HTTP/1.0), they willbe silently discarded.</p><p>Note that HTTP requires the <code>Trailer</code> header to be sent if you intend toemit trailers, with a list of the header fields in its value. E.g.,</p><pre><code>response.writeHead(200, { &#39;Content-Type&#39;: &#39;text/plain&#39;,                          &#39;Trailer&#39;: &#39;Content-MD5&#39; });response.write(fileData);response.addTrailers({&#39;Content-MD5&#39;: &quot;7895bf4b8828b55ceaf47747b4bca667&quot;});response.end();</code></pre></summary>
            /// <signature>
            /// <param name="headers"></param>
            /// </signature>
        }
        this.end = function(data, encoding) {
            /// <summary><p>This method signals to the server that all of the response headers and bodyhave been sent; that server should consider this message complete.The method, <code>response.end()</code>, MUST be called on eachresponse.</p><p>If <code>data</code> is specified, it is equivalent to calling <code>response.write(data, encoding)</code>followed by <code>response.end()</code>.</p></summary>
            /// <signature>
            /// <param name="data"></param>
            /// <param name="encoding"></param>
            /// </signature>
        }
        emitter = new Events().EventEmitter;
        /// <field name='close'><p><code>function () { }</code></p><p>Indicates that the underlaying connection was terminated before<code>response.end()</code> was called or able to flush.</p></field>
        this.close = new emitter();
        /// <field name='statusCode'><p>When using implicit headers (not calling <code>response.writeHead()</code> explicitly), this propertycontrols the status code that will be sent to the client when the headers getflushed.</p><p>Example:</p><pre><code>response.statusCode = 404;</code></pre><p>After response header was sent to the client, this property indicates thestatus code which was sent out.</p></field>
        this.statusCode = undefined;
        /// <field name='sendDate'><p>When true, the Date header will be automatically generated and sent in the response if it is not already present in the headers. Defaults to true.</p><p>This should only be disabled for testing; HTTP requires the Date headerin responses.</p></field>
        this.sendDate = undefined;
    }
    this.Agent = function() {
        /// <field name='maxSockets'><p>By default set to 5. Determines how many concurrent sockets the agent can have open per host.</p></field>
        this.maxSockets = undefined;
        /// <field name='sockets'><p>An object which contains arrays of sockets currently in use by the Agent. Do not modify.</p></field>
        this.sockets = undefined;
        /// <field name='requests'><p>An object which contains queues of requests that have not yet been assigned to sockets. Do not modify.</p></field>
        this.requests = undefined;
    }
    this.ClientRequest = function() {
        this.write = function(chunk, encoding) {
            /// <summary><p>Sends a chunk of the body.  By calling this methodmany times, the user can stream a request body to aserver--in that case it is suggested to use the<code>[&#39;Transfer-Encoding&#39;, &#39;chunked&#39;]</code> header line whencreating the request.</p><p>The <code>chunk</code> argument should be a [Buffer][] or a string.</p><p>The <code>encoding</code> argument is optional and only applies when <code>chunk</code> is a string.Defaults to <code>&#39;utf8&#39;</code>.</p></summary>
            /// <signature>
            /// <param name="chunk"></param>
            /// <param name="encoding"></param>
            /// </signature>
        }
        this.end = function(data, encoding) {
            /// <summary><p>Finishes sending the request. If any parts of the body areunsent, it will flush them to the stream. If the request ischunked, this will send the terminating <code>&#39;0\r\n\r\n&#39;</code>.</p><p>If <code>data</code> is specified, it is equivalent to calling<code>request.write(data, encoding)</code> followed by <code>request.end()</code>.</p></summary>
            /// <signature>
            /// <param name="data"></param>
            /// <param name="encoding"></param>
            /// </signature>
        }
        this.abort = function() {
            /// <summary><p>Aborts a request.  (New since v0.3.8.)</p></summary>
        }
        this.setTimeout = function(timeout, callback) {
            /// <summary><p>Once a socket is assigned to this request and is connected[socket.setTimeout()][] will be called.</p></summary>
            /// <signature>
            /// <param name="timeout"></param>
            /// <param name="callback"></param>
            /// </signature>
        }
        this.setNoDelay = function(noDelay) {
            /// <summary><p>Once a socket is assigned to this request and is connected[socket.setNoDelay()][] will be called.</p></summary>
            /// <signature>
            /// <param name="noDelay"></param>
            /// </signature>
        }
        this.setSocketKeepAlive = function(enable, initialDelay) {
            /// <summary><p>Once a socket is assigned to this request and is connected[socket.setKeepAlive()][] will be called.</p></summary>
            /// <signature>
            /// <param name="enable"></param>
            /// <param name="initialDelay"></param>
            /// </signature>
        }
        emitter = new Events().EventEmitter;
        /// <field name='response'><p><code>function (response) { }</code></p><p>Emitted when a response is received to this request. This event is emitted onlyonce. The <code>response</code> argument will be an instance of <code>http.ClientResponse</code>.</p><p>Options:</p><ul><li><code>host</code>: A domain name or IP address of the server to issue the request to.</li><li><code>port</code>: Port of remote server.</li><li><code>socketPath</code>: Unix Domain Socket (use one of host:port or socketPath)</li></ul></field>
        this.response = new emitter();
        /// <field name='socket'><p><code>function (socket) { }</code></p><p>Emitted after a socket is assigned to this request.</p></field>
        this.socket = new emitter();
        /// <field name='connect'><p><code>function (response, socket, head) { }</code></p><p>Emitted each time a server responds to a request with a CONNECT method. If thisevent isn&#39;t being listened for, clients receiving a CONNECT method will havetheir connections closed.</p><p>A client server pair that show you how to listen for the <code>connect</code> event.</p><pre><code>var http = require(&#39;http&#39;);var net = require(&#39;net&#39;);var url = require(&#39;url&#39;);// Create an HTTP tunneling proxyvar proxy = http.createServer(function (req, res) {  res.writeHead(200, {&#39;Content-Type&#39;: &#39;text/plain&#39;});  res.end(&#39;okay&#39;);});proxy.on(&#39;connect&#39;, function(req, cltSocket, head) {  // connect to an origin server  var srvUrl = url.parse(&#39;http://&#39; + req.url);  var srvSocket = net.connect(srvUrl.port, srvUrl.hostname, function() {    cltSocket.write(&#39;HTTP/1.1 200 Connection Established\r\n&#39; +                    &#39;Proxy-agent: Node-Proxy\r\n&#39; +                    &#39;\r\n&#39;);    srvSocket.write(head);    srvSocket.pipe(cltSocket);    cltSocket.pipe(srvSocket);  });});// now that proxy is runningproxy.listen(1337, &#39;127.0.0.1&#39;, function() {  // make a request to a tunneling proxy  var options = {    port: 1337,    host: &#39;127.0.0.1&#39;,    method: &#39;CONNECT&#39;,    path: &#39;www.google.com:80&#39;  };  var req = http.request(options);  req.end();  req.on(&#39;connect&#39;, function(res, socket, head) {    console.log(&#39;got connected!&#39;);    // make a request over an HTTP tunnel    socket.write(&#39;GET / HTTP/1.1\r\n&#39; +                 &#39;Host: www.google.com:80\r\n&#39; +                 &#39;Connection: close\r\n&#39; +                 &#39;\r\n&#39;);    socket.on(&#39;data&#39;, function(chunk) {      console.log(chunk.toString());    });    socket.on(&#39;end&#39;, function() {      proxy.close();    });  });});</code></pre></field>
        this.connect = new emitter();
        /// <field name='upgrade'><p><code>function (response, socket, head) { }</code></p><p>Emitted each time a server responds to a request with an upgrade. If thisevent isn&#39;t being listened for, clients receiving an upgrade header will havetheir connections closed.</p><p>A client server pair that show you how to listen for the <code>upgrade</code> event.</p><pre><code>var http = require(&#39;http&#39;);// Create an HTTP servervar srv = http.createServer(function (req, res) {  res.writeHead(200, {&#39;Content-Type&#39;: &#39;text/plain&#39;});  res.end(&#39;okay&#39;);});srv.on(&#39;upgrade&#39;, function(req, socket, head) {  socket.write(&#39;HTTP/1.1 101 Web Socket Protocol Handshake\r\n&#39; +               &#39;Upgrade: WebSocket\r\n&#39; +               &#39;Connection: Upgrade\r\n&#39; +               &#39;\r\n&#39;);  socket.pipe(socket); // echo back});// now that server is runningsrv.listen(1337, &#39;127.0.0.1&#39;, function() {  // make a request  var options = {    port: 1337,    host: &#39;127.0.0.1&#39;,    headers: {      &#39;Connection&#39;: &#39;Upgrade&#39;,      &#39;Upgrade&#39;: &#39;websocket&#39;    }  };  var req = http.request(options);  req.end();  req.on(&#39;upgrade&#39;, function(res, socket, upgradeHead) {    console.log(&#39;got upgraded!&#39;);    socket.end();    process.exit(0);  });});</code></pre></field>
        this.upgrade = new emitter();
        /// <field name='continue'><p><code>function () { }</code></p><p>Emitted when the server sends a &#39;100 Continue&#39; HTTP response, usually becausethe request contained &#39;Expect: 100-continue&#39;. This is an instruction thatthe client should send the request body.</p></field>
        this.continue = new emitter();
    }
    /// <field name='STATUS_CODES'><p>A collection of all the standard HTTP response status codes, and theshort description of each.  For example, <code>http.STATUS_CODES[404] === &#39;NotFound&#39;</code>.</p></field>
    this.STATUS_CODES = undefined;
    /// <field name='globalAgent'><p>Global instance of Agent which is used as the default for all http clientrequests.</p></field>
    this.globalAgent = undefined;
    /// <field name='ClientResponse'><p>This object is created when making a request with <code>http.request()</code>. It ispassed to the <code>&#39;response&#39;</code> event of the request object.</p><p>The response implements the [Readable Stream][] interface. This is an[EventEmitter][] with the following events:</p></field>
    this.ClientResponse = undefined;
} // module http

function https() {
    /// <summary><p>HTTPS is the HTTP protocol over TLS/SSL. In Node this is implemented as aseparate module.</p></summary>
    this.createServer = function(options, requestListener) {
        /// <summary><p>Returns a new HTTPS web server object. The <code>options</code> is similar to[tls.createServer()][].  The <code>requestListener</code> is a function which isautomatically added to the <code>&#39;request&#39;</code> event.</p><p>Example:</p><pre><code>// curl -k https://localhost:8000/var https = require(&#39;https&#39;);var fs = require(&#39;fs&#39;);var options = {  key: fs.readFileSync(&#39;test/fixtures/keys/agent2-key.pem&#39;),  cert: fs.readFileSync(&#39;test/fixtures/keys/agent2-cert.pem&#39;)};https.createServer(options, function (req, res) {  res.writeHead(200);  res.end(&quot;hello world\n&quot;);}).listen(8000);</code></pre><p>Or</p><pre><code>var https = require(&#39;https&#39;);var fs = require(&#39;fs&#39;);var options = {  pfx: fs.readFileSync(&#39;server.pfx&#39;)};https.createServer(options, function (req, res) {  res.writeHead(200);  res.end(&quot;hello world\n&quot;);}).listen(8000);</code></pre></summary>
        /// <signature>
        /// <param name="options"></param>
        /// <param name="requestListener"></param>
        /// </signature>
        return new https.Server();
    }
    this.request = function(options, callback) {
        /// <summary><p>Makes a request to a secure web server.</p><p><code>options</code> can be an object or a string. If <code>options</code> is a string, it isautomatically parsed with <a href="url.html#url.parse">url.parse()</a>.</p><p>All options from [http.request()][] are valid.</p><p>Example:</p><pre><code>var https = require(&#39;https&#39;);var options = {  host: &#39;encrypted.google.com&#39;,  port: 443,  path: &#39;/&#39;,  method: &#39;GET&#39;};var req = https.request(options, function(res) {  console.log(&quot;statusCode: &quot;, res.statusCode);  console.log(&quot;headers: &quot;, res.headers);  res.on(&#39;data&#39;, function(d) {    process.stdout.write(d);  });});req.end();req.on(&#39;error&#39;, function(e) {  console.error(e);});</code></pre><p>The options argument has the following options</p><ul><li>host: IP or domain of host to make request to. Defaults to <code>&#39;localhost&#39;</code>.</li><li>port: port of host to request to. Defaults to 443.</li><li>path: Path to request. Default <code>&#39;/&#39;</code>.</li><li><p>method: HTTP request method. Default <code>&#39;GET&#39;</code>.</p></li><li><p><code>host</code>: A domain name or IP address of the server to issue the request to.Defaults to <code>&#39;localhost&#39;</code>.</p></li><li><code>hostname</code>: To support <code>url.parse()</code> <code>hostname</code> is preferred over <code>host</code></li><li><code>port</code>: Port of remote server. Defaults to 443.</li><li><code>method</code>: A string specifying the HTTP request method. Defaults to <code>&#39;GET&#39;</code>.</li><li><code>path</code>: Request path. Defaults to <code>&#39;/&#39;</code>. Should include query string if any.E.G. <code>&#39;/index.html?page=12&#39;</code></li><li><code>headers</code>: An object containing request headers.</li><li><code>auth</code>: Basic authentication i.e. <code>&#39;user:password&#39;</code> to compute anAuthorization header.</li><li><code>agent</code>: Controls [Agent][] behavior. When an Agent is used request willdefault to <code>Connection: keep-alive</code>. Possible values:<ul><li><code>undefined</code> (default): use [globalAgent][] for this host and port.</li><li><code>Agent</code> object: explicitly use the passed in <code>Agent</code>.</li><li><code>false</code>: opts out of connection pooling with an Agent, defaults request to<code>Connection: close</code>.</li></ul></li></ul><p>The following options from [tls.connect()][] can also be specified. However, a[globalAgent][] silently ignores these.</p><ul><li><code>pfx</code>: Certificate, Private key and CA certificates to use for SSL. Default <code>null</code>.</li><li><code>key</code>: Private key to use for SSL. Default <code>null</code>.</li><li><code>passphrase</code>: A string of passphrase for the private key or pfx. Default <code>null</code>.</li><li><code>cert</code>: Public x509 certificate to use. Default <code>null</code>.</li><li><code>ca</code>: An authority certificate or array of authority certificates to checkthe remote host against.</li><li><code>ciphers</code>: A string describing the ciphers to use or exclude. Consult<a href="http://www.openssl.org/docs/apps/ciphers.html#CIPHER_LIST_FORMAT">http://www.openssl.org/docs/apps/ciphers.html#CIPHER_LIST_FORMAT</a> fordetails on the format.</li><li><code>rejectUnauthorized</code>: If <code>true</code>, the server certificate is verified againstthe list of supplied CAs. An <code>&#39;error&#39;</code> event is emitted if verificationfails. Verification happens at the connection level, <em>before</em> the HTTPrequest is sent. Default <code>false</code>.</li></ul><p>In order to specify these options, use a custom <code>Agent</code>.</p><p>Example:</p><pre><code>var options = {  host: &#39;encrypted.google.com&#39;,  port: 443,  path: &#39;/&#39;,  method: &#39;GET&#39;,  key: fs.readFileSync(&#39;test/fixtures/keys/agent2-key.pem&#39;),  cert: fs.readFileSync(&#39;test/fixtures/keys/agent2-cert.pem&#39;)};options.agent = new https.Agent(options);var req = https.request(options, function(res) {  ...}</code></pre><p>Or does not use an <code>Agent</code>.</p><p>Example:</p><pre><code>var options = {  host: &#39;encrypted.google.com&#39;,  port: 443,  path: &#39;/&#39;,  method: &#39;GET&#39;,  key: fs.readFileSync(&#39;test/fixtures/keys/agent2-key.pem&#39;),  cert: fs.readFileSync(&#39;test/fixtures/keys/agent2-cert.pem&#39;),  agent: false};var req = https.request(options, function(res) {  ...}</code></pre></summary>
        /// <signature>
        /// <param name="options"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.get = function(options, callback) {
        /// <summary><p>Like <code>http.get()</code> but for HTTPS.</p><p><code>options</code> can be an object or a string. If <code>options</code> is a string, it isautomatically parsed with <a href="url.html#url.parse">url.parse()</a>.</p><p>Example:</p><pre><code>var https = require(&#39;https&#39;);https.get(&#39;https://encrypted.google.com/&#39;, function(res) {  console.log(&quot;statusCode: &quot;, res.statusCode);  console.log(&quot;headers: &quot;, res.headers);  res.on(&#39;data&#39;, function(d) {    process.stdout.write(d);  });}).on(&#39;error&#39;, function(e) {  console.error(e);});</code></pre></summary>
        /// <signature>
        /// <param name="options"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.Server = function() {
    }
    this.Agent = function() {
    }
    /// <field name='globalAgent'><p>Global instance of [https.Agent][] for all HTTPS client requests.</p></field>
    this.globalAgent = undefined;
} // module https

function url() {
    /// <summary><p>This module has utilities for URL resolution and parsing.Call <code>require(&#39;url&#39;)</code> to use it.</p><p>Parsed URL objects have some or all of the following fields, depending onwhether or not they exist in the URL string. Any parts that are not in the URLstring will not be in the parsed object. Examples are shown for the URL</p><p><code>&#39;http://user:pass@host.com:8080/p/a/t/h?query=string#hash&#39;</code></p><ul><li><p><code>href</code>: The full URL that was originally parsed. Both the protocol and host are lowercased.</p><p>  Example: <code>&#39;http://user:pass@host.com:8080/p/a/t/h?query=string#hash&#39;</code></p></li><li><p><code>protocol</code>: The request protocol, lowercased.</p><p>  Example: <code>&#39;http:&#39;</code></p></li><li><p><code>host</code>: The full lowercased host portion of the URL, including portinformation.</p><p>  Example: <code>&#39;host.com:8080&#39;</code></p></li><li><p><code>auth</code>: The authentication information portion of a URL.</p><p>  Example: <code>&#39;user:pass&#39;</code></p></li><li><p><code>hostname</code>: Just the lowercased hostname portion of the host.</p><p>  Example: <code>&#39;host.com&#39;</code></p></li><li><p><code>port</code>: The port number portion of the host.</p><p>  Example: <code>&#39;8080&#39;</code></p></li><li><p><code>pathname</code>: The path section of the URL, that comes after the host andbefore the query, including the initial slash if present.</p><p>  Example: <code>&#39;/p/a/t/h&#39;</code></p></li><li><p><code>search</code>: The &#39;query string&#39; portion of the URL, including the leadingquestion mark.</p><p>  Example: <code>&#39;?query=string&#39;</code></p></li><li><p><code>path</code>: Concatenation of <code>pathname</code> and <code>search</code>.</p><p>  Example: <code>&#39;/p/a/t/h?query=string&#39;</code></p></li><li><p><code>query</code>: Either the &#39;params&#39; portion of the query string, or aquerystring-parsed object.</p><p>  Example: <code>&#39;query=string&#39;</code> or <code>{&#39;query&#39;:&#39;string&#39;}</code></p></li><li><p><code>hash</code>: The &#39;fragment&#39; portion of the URL including the pound-sign.</p><p>  Example: <code>&#39;#hash&#39;</code></p></li></ul><p>The following methods are provided by the URL module:</p></summary>
    this.parse = function(urlStr, parseQueryString, slashesDenoteHost) {
        /// <summary><p>Take a URL string, and return an object.</p><p>Pass <code>true</code> as the second argument to also parsethe query string using the <code>querystring</code> module.Defaults to <code>false</code>.</p><p>Pass <code>true</code> as the third argument to treat <code>//foo/bar</code> as<code>{ host: &#39;foo&#39;, pathname: &#39;/bar&#39; }</code> rather than<code>{ pathname: &#39;//foo/bar&#39; }</code>. Defaults to <code>false</code>.</p></summary>
        /// <signature>
        /// <param name="urlStr"></param>
        /// <param name="parseQueryString"></param>
        /// <param name="slashesDenoteHost"></param>
        /// </signature>
    }
    this.format = function(urlObj) {
        /// <summary><p>Take a parsed URL object, and return a formatted URL string.</p><ul><li><code>href</code> will be ignored.</li><li><code>protocol</code>is treated the same with or without the trailing <code>:</code> (colon).<ul><li>The protocols <code>http</code>, <code>https</code>, <code>ftp</code>, <code>gopher</code>, <code>file</code> will bepostfixed with <code>://</code> (colon-slash-slash).</li><li>All other protocols <code>mailto</code>, <code>xmpp</code>, <code>aim</code>, <code>sftp</code>, <code>foo</code>, etc willbe postfixed with <code>:</code> (colon)</li></ul></li><li><code>auth</code> will be used if present.</li><li><code>hostname</code> will only be used if <code>host</code> is absent.</li><li><code>port</code> will only be used if <code>host</code> is absent.</li><li><code>host</code> will be used in place of <code>hostname</code> and <code>port</code></li><li><code>pathname</code> is treated the same with or without the leading <code>/</code> (slash)</li><li><code>search</code> will be used in place of <code>query</code></li><li><code>query</code> (object; see <code>querystring</code>) will only be used if <code>search</code> is absent.</li><li><code>search</code> is treated the same with or without the leading <code>?</code> (question mark)</li><li><code>hash</code> is treated the same with or without the leading <code>#</code> (pound sign, anchor)</li></ul></summary>
        /// <signature>
        /// <param name="urlObj"></param>
        /// </signature>
    }
    this.resolve = function(from, to) {
        /// <summary><p>Take a base URL, and a href URL, and resolve them as a browser would foran anchor tag.</p></summary>
        /// <signature>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// </signature>
    }
} // module url

function querystring() {
    /// <summary><p>This module provides utilities for dealing with query strings.It provides the following methods:</p></summary>
    this.stringify = function(obj, sep, eq) {
        /// <summary><p>Serialize an object to a query string.Optionally override the default separator (<code>&#39;&amp;&#39;</code>) and assignment (<code>&#39;=&#39;</code>)characters.</p><p>Example:</p><pre><code>querystring.stringify({ foo: &#39;bar&#39;, baz: [&#39;qux&#39;, &#39;quux&#39;], corge: &#39;&#39; })// returns&#39;foo=bar&amp;baz=qux&amp;baz=quux&amp;corge=&#39;querystring.stringify({foo: &#39;bar&#39;, baz: &#39;qux&#39;}, &#39;;&#39;, &#39;:&#39;)// returns&#39;foo:bar;baz:qux&#39;</code></pre></summary>
        /// <signature>
        /// <param name="obj"></param>
        /// <param name="sep"></param>
        /// <param name="eq"></param>
        /// </signature>
    }
    this.parse = function(str, sep, eq, options) {
        /// <summary><p>Deserialize a query string to an object.Optionally override the default separator (<code>&#39;&amp;&#39;</code>) and assignment (<code>&#39;=&#39;</code>)characters.</p><p>Options object may contain <code>maxKeys</code> property (equal to 1000 by default), it&#39;llbe used to limit processed keys. Set it to 0 to remove key count limitation.</p><p>Example:</p><pre><code>querystring.parse(&#39;foo=bar&amp;baz=qux&amp;baz=quux&amp;corge&#39;)// returns{ foo: &#39;bar&#39;, baz: [&#39;qux&#39;, &#39;quux&#39;], corge: &#39;&#39; }</code></pre></summary>
        /// <signature>
        /// <param name="str"></param>
        /// <param name="sep"></param>
        /// <param name="eq"></param>
        /// <param name="options"></param>
        /// </signature>
    }
    /// <field name='escape'><p>The escape function used by <code>querystring.stringify</code>,provided so that it could be overridden if necessary.</p></field>
    this.escape = undefined;
    /// <field name='unescape'><p>The unescape function used by <code>querystring.parse</code>,provided so that it could be overridden if necessary.</p></field>
    this.unescape = undefined;
} // module querystring

function punycode() {
    /// <summary><p><a href="http://mths.be/punycode">Punycode.js</a> is bundled with Node.js v0.6.2+. Use<code>require(&#39;punycode&#39;)</code> to access it. (To use it with other Node.js versions,use npm to install the <code>punycode</code> module first.)</p></summary>
    this.decode = function(string) {
        /// <summary><p>Converts a Punycode string of ASCII code points to a string of Unicode codepoints.</p><pre><code>// decode domain name partspunycode.decode(&#39;maana-pta&#39;); // &#39;maÃ±ana&#39;punycode.decode(&#39;--dqo34k&#39;); // &#39;â˜ƒ-âŒ˜&#39;</code></pre></summary>
        /// <signature>
        /// <param name="string"></param>
        /// </signature>
    }
    this.encode = function(string) {
        /// <summary><p>Converts a string of Unicode code points to a Punycode string of ASCII codepoints.</p><pre><code>// encode domain name partspunycode.encode(&#39;maÃ±ana&#39;); // &#39;maana-pta&#39;punycode.encode(&#39;â˜ƒ-âŒ˜&#39;); // &#39;--dqo34k&#39;</code></pre></summary>
        /// <signature>
        /// <param name="string"></param>
        /// </signature>
    }
    this.toUnicode = function(domain) {
        /// <summary><p>Converts a Punycode string representing a domain name to Unicode. Only thePunycoded parts of the domain name will be converted, i.e. it doesn&#39;t matter ifyou call it on a string that has already been converted to Unicode.</p><pre><code>// decode domain namespunycode.toUnicode(&#39;xn--maana-pta.com&#39;); // &#39;maÃ±ana.com&#39;punycode.toUnicode(&#39;xn----dqo34k.com&#39;); // &#39;â˜ƒ-âŒ˜.com&#39;</code></pre></summary>
        /// <signature>
        /// <param name="domain"></param>
        /// </signature>
    }
    this.toASCII = function(domain) {
        /// <summary><p>Converts a Unicode string representing a domain name to Punycode. Only thenon-ASCII parts of the domain name will be converted, i.e. it doesn&#39;t matter ifyou call it with a domain that&#39;s already in ASCII.</p><pre><code>// encode domain namespunycode.toASCII(&#39;maÃ±ana.com&#39;); // &#39;xn--maana-pta.com&#39;punycode.toASCII(&#39;â˜ƒ-âŒ˜.com&#39;); // &#39;xn----dqo34k.com&#39;</code></pre></summary>
        /// <signature>
        /// <param name="domain"></param>
        /// </signature>
    }
    this.ucs2 = undefined;
    /// <field name='version'><p>A string representing the current Punycode.js version number.</p></field>
    this.version = undefined;
} // module punycode

function readline() {
    /// <summary><p>To use this module, do <code>require(&#39;readline&#39;)</code>. Readline allows reading of astream (such as <code>process.stdin</code>) on a line-by-line basis.</p><p>Note that once you&#39;ve invoked this module, your node program will notterminate until you&#39;ve closed the interface. Here&#39;s how to allow yourprogram to gracefully exit:</p><pre><code>var readline = require(&#39;readline&#39;);var rl = readline.createInterface({  input: process.stdin,  output: process.stdout});rl.question(&quot;What do you think of node.js? &quot;, function(answer) {  // TODO: Log the answer in a database  console.log(&quot;Thank you for your valuable feedback:&quot;, answer);  rl.close();});</code></pre></summary>
    this.createInterface = function(options) {
        /// <summary><p>Creates a readline <code>Interface</code> instance. Accepts an &quot;options&quot; Object that takesthe following values:</p><ul><li><p><code>input</code> - the readable stream to listen to (Required).</p></li><li><p><code>output</code> - the writable stream to write readline data to (Required).</p></li><li><p><code>completer</code> - an optional function that is used for Tab autocompletion. Seebelow for an example of using this.</p></li><li><p><code>terminal</code> - pass <code>true</code> if the <code>input</code> and <code>output</code> streams should betreated like a TTY, and have ANSI/VT100 escape codes written to it.Defaults to checking <code>isTTY</code> on the <code>output</code> stream upon instantiation.</p></li></ul><p>The <code>completer</code> function is given a the current line entered by the user, andis supposed to return an Array with 2 entries:</p><ol><li><p>An Array with matching entries for the completion.</p></li><li><p>The substring that was used for the matching.</p></li></ol><p>Which ends up looking something like:<code>[[substr1, substr2, ...], originalsubstring]</code>.</p><p>Example:</p><pre><code>function completer(line) {  var completions = &#39;.help .error .exit .quit .q&#39;.split(&#39; &#39;)  var hits = completions.filter(function(c) { return c.indexOf(line) == 0 })  // show all completions if none found  return [hits.length ? hits : completions, line]}</code></pre><p>Also <code>completer</code> can be run in async mode if it accepts two arguments:</p><pre><code>function completer(linePartial, callback) {  callback(null, [[&#39;123&#39;], linePartial]);}</code></pre><p><code>createInterface</code> is commonly used with <code>process.stdin</code> and<code>process.stdout</code> in order to accept user input:</p><pre><code>var readline = require(&#39;readline&#39;);var rl = readline.createInterface({  input: process.stdin,  output: process.stdout});</code></pre><p>Once you have a readline instance, you most commonly listen for the<code>&quot;line&quot;</code> event.</p><p>If <code>terminal</code> is <code>true</code> for this instance then the <code>output</code> stream will getthe best compatibility if it defines an <code>output.columns</code> property, and firesa <code>&quot;resize&quot;</code> event on the <code>output</code> if/when the columns ever change(<code>process.stdout</code> does this automatically when it is a TTY).</p></summary>
        /// <signature>
        /// <param name="options"></param>
        /// </signature>
        return new readline.Interface();
    }
    this.Interface = function() {
        this.setPrompt = function(prompt, length) {
            /// <summary><p>Sets the prompt, for example when you run <code>node</code> on the command line, you see<code>&gt; </code>, which is node&#39;s prompt.</p></summary>
            /// <signature>
            /// <param name="prompt"></param>
            /// <param name="length"></param>
            /// </signature>
        }
        this.prompt = function(preserveCursor) {
            /// <summary><p>Readies readline for input from the user, putting the current <code>setPrompt</code>options on a new line, giving the user a new spot to write. Set <code>preserveCursor</code>to <code>true</code> to prevent the cursor placement being reset to <code>0</code>.</p><p>This will also resume the <code>input</code> stream used with <code>createInterface</code> if it hasbeen paused.</p></summary>
            /// <signature>
            /// <param name="preserveCursor"></param>
            /// </signature>
        }
        this.question = function(query, callback) {
            /// <summary><p>Prepends the prompt with <code>query</code> and invokes <code>callback</code> with the user&#39;sresponse. Displays the query to the user, and then invokes <code>callback</code>with the user&#39;s response after it has been typed.</p><p>This will also resume the <code>input</code> stream used with <code>createInterface</code> ifit has been paused.</p><p>Example usage:</p><pre><code>interface.question(&#39;What is your favorite food?&#39;, function(answer) {  console.log(&#39;Oh, so your favorite food is &#39; + answer);});</code></pre></summary>
            /// <signature>
            /// <param name="query"></param>
            /// <param name="callback"></param>
            /// </signature>
        }
        this.pause = function() {
            /// <summary><p>Pauses the readline <code>input</code> stream, allowing it to be resumed later if needed.</p></summary>
        }
        this.resume = function() {
            /// <summary><p>Resumes the readline <code>input</code> stream.</p></summary>
        }
        this.close = function() {
            /// <summary><p>Closes the <code>Interface</code> instance, relinquishing control on the <code>input</code> and<code>output</code> streams. The &quot;close&quot; event will also be emitted.</p></summary>
        }
        this.write = function(data, key) {
            /// <summary><p>Writes <code>data</code> to <code>output</code> stream. <code>key</code> is an object literal to represent a keysequence; available if the terminal is a TTY.</p><p>This will also resume the <code>input</code> stream if it has been paused.</p><p>Example:</p><pre><code>rl.write(&#39;Delete me!&#39;);// Simulate ctrl+u to delete the line written previouslyrl.write(null, {ctrl: true, name: &#39;u&#39;});</code></pre></summary>
            /// <signature>
            /// <param name="data"></param>
            /// <param name="key"></param>
            /// </signature>
        }
    }
} // module readline

function repl() {
    /// <summary><p>A Read-Eval-Print-Loop (REPL) is available both as a standalone program andeasily includable in other programs. The REPL provides a way to interactivelyrun JavaScript and see the results.  It can be used for debugging, testing, orjust trying things out.</p><p>By executing <code>node</code> without any arguments from the command-line you will bedropped into the REPL. It has simplistic emacs line-editing.</p><pre><code>mjr:~$ nodeType &#39;.help&#39; for options.&gt; a = [ 1, 2, 3];[ 1, 2, 3 ]&gt; a.forEach(function (v) {...   console.log(v);...   });123</code></pre><p>For advanced line-editors, start node with the environmental variable<code>NODE_NO_READLINE=1</code>. This will start the main and debugger REPL in canonicalterminal settings which will allow you to use with <code>rlwrap</code>.</p><p>For example, you could add this to your bashrc file:</p><pre><code>alias node=&quot;env NODE_NO_READLINE=1 rlwrap node&quot;</code></pre></summary>
    this.start = function(options) {
        /// <summary><p>Returns and starts a <code>REPLServer</code> instance. Accepts an &quot;options&quot; Object thattakes the following values:</p><ul><li><p><code>prompt</code> - the prompt and <code>stream</code> for all I/O. Defaults to <code>&gt; </code>.</p></li><li><p><code>input</code> - the readable stream to listen to. Defaults to <code>process.stdin</code>.</p></li><li><p><code>output</code> - the writable stream to write readline data to. Defaults to<code>process.stdout</code>.</p></li><li><p><code>terminal</code> - pass <code>true</code> if the <code>stream</code> should be treated like a TTY, andhave ANSI/VT100 escape codes written to it. Defaults to checking <code>isTTY</code>on the <code>output</code> stream upon instantiation.</p></li><li><p><code>eval</code> - function that will be used to eval each given line. Defaults toan async wrapper for <code>eval()</code>. See below for an example of a custom <code>eval</code>.</p></li><li><p><code>useColors</code> - a boolean which specifies whether or not the <code>writer</code> functionshould output colors. If a different <code>writer</code> function is set then this doesnothing. Defaults to the repl&#39;s <code>terminal</code> value.</p></li><li><p><code>useGlobal</code> - if set to <code>true</code>, then the repl will use the <code>global</code> object,instead of running scripts in a separate context. Defaults to <code>false</code>.</p></li><li><p><code>ignoreUndefined</code> - if set to <code>true</code>, then the repl will not output thereturn value of command if it&#39;s <code>undefined</code>. Defaults to <code>false</code>.</p></li><li><p><code>writer</code> - the function to invoke for each command that gets evaluated whichreturns the formatting (including coloring) to display. Defaults to<code>util.inspect</code>.</p></li></ul><p>You can use your own <code>eval</code> function if it has following signature:</p><pre><code>function eval(cmd, context, filename, callback) {  callback(null, result);}</code></pre><p>Multiple REPLs may be started against the same running instance of node.  Eachwill share the same global object but will have unique I/O.</p><p>Here is an example that starts a REPL on stdin, a Unix socket, and a TCP socket:</p><pre><code>var net = require(&quot;net&quot;),    repl = require(&quot;repl&quot;);connections = 0;repl.start({  prompt: &quot;node via stdin&gt; &quot;,  input: process.stdin,  output: process.stdout});net.createServer(function (socket) {  connections += 1;  repl.start({    prompt: &quot;node via Unix socket&gt; &quot;,    input: socket,    output: socket  }).on(&#39;exit&#39;, function() {    socket.end();  })}).listen(&quot;/tmp/node-repl-sock&quot;);net.createServer(function (socket) {  connections += 1;  repl.start({    prompt: &quot;node via TCP socket&gt; &quot;,    input: socket,    output: socket  }).on(&#39;exit&#39;, function() {    socket.end();  });}).listen(5001);</code></pre><p>Running this program from the command line will start a REPL on stdin.  OtherREPL clients may connect through the Unix socket or TCP socket. <code>telnet</code> is usefulfor connecting to TCP sockets, and <code>socat</code> can be used to connect to both Unix andTCP sockets.</p><p>By starting a REPL from a Unix socket-based server instead of stdin, you canconnect to a long-running node process without restarting it.</p><p>For an example of running a &quot;full-featured&quot; (<code>terminal</code>) REPL overa <code>net.Server</code> and <code>net.Socket</code> instance, see: <a href="https://gist.github.com/2209310">https://gist.github.com/2209310</a></p><p>For an example of running a REPL instance over <code>curl(1)</code>,see: <a href="https://gist.github.com/2053342">https://gist.github.com/2053342</a></p></summary>
        /// <signature>
        /// <param name="options"></param>
        /// </signature>
    }
} // module repl

function vm() {
    /// <summary><p>You can access this module with:</p><pre><code>var vm = require(&#39;vm&#39;);</code></pre><p>JavaScript code can be compiled and run immediately or compiled, saved, and run later.</p></summary>
    this.runInThisContext = function(code, filename) {
        /// <summary><p><code>vm.runInThisContext()</code> compiles <code>code</code>, runs it and returns the result. Runningcode does not have access to local scope. <code>filename</code> is optional, it&#39;s used onlyin stack traces.</p><p>Example of using <code>vm.runInThisContext</code> and <code>eval</code> to run the same code:</p><pre><code>var localVar = 123,    usingscript, evaled,    vm = require(&#39;vm&#39;);usingscript = vm.runInThisContext(&#39;localVar = 1;&#39;,  &#39;myfile.vm&#39;);console.log(&#39;localVar: &#39; + localVar + &#39;, usingscript: &#39; +  usingscript);evaled = eval(&#39;localVar = 1;&#39;);console.log(&#39;localVar: &#39; + localVar + &#39;, evaled: &#39; +  evaled);// localVar: 123, usingscript: 1// localVar: 1, evaled: 1</code></pre><p><code>vm.runInThisContext</code> does not have access to the local scope, so <code>localVar</code> is unchanged.<code>eval</code> does have access to the local scope, so <code>localVar</code> is changed.</p><p>In case of syntax error in <code>code</code>, <code>vm.runInThisContext</code> emits the syntax error to stderrand throws an exception.</p></summary>
        /// <signature>
        /// <param name="code"></param>
        /// <param name="filename"></param>
        /// </signature>
    }
    this.runInNewContext = function(code, sandbox, filename) {
        /// <summary><p><code>vm.runInNewContext</code> compiles <code>code</code>, then runs it in <code>sandbox</code> and returns theresult. Running code does not have access to local scope. The object <code>sandbox</code>will be used as the global object for <code>code</code>.<code>sandbox</code> and <code>filename</code> are optional, <code>filename</code> is only used in stack traces.</p><p>Example: compile and execute code that increments a global variable and sets a new one.These globals are contained in the sandbox.</p><pre><code>var util = require(&#39;util&#39;),    vm = require(&#39;vm&#39;),    sandbox = {      animal: &#39;cat&#39;,      count: 2    };vm.runInNewContext(&#39;count += 1; name = &quot;kitty&quot;&#39;, sandbox, &#39;myfile.vm&#39;);console.log(util.inspect(sandbox));// { animal: &#39;cat&#39;, count: 3, name: &#39;kitty&#39; }</code></pre><p>Note that running untrusted code is a tricky business requiring great care.  To prevent accidentalglobal variable leakage, <code>vm.runInNewContext</code> is quite useful, but safely running untrusted coderequires a separate process.</p><p>In case of syntax error in <code>code</code>, <code>vm.runInNewContext</code> emits the syntax error to stderrand throws an exception.</p></summary>
        /// <signature>
        /// <param name="code"></param>
        /// <param name="sandbox"></param>
        /// <param name="filename"></param>
        /// </signature>
    }
    this.runInContext = function(code, context, filename) {
        /// <summary><p><code>vm.runInContext</code> compiles <code>code</code>, then runs it in <code>context</code> and returns theresult. A (V8) context comprises a global object, together with a set ofbuilt-in objects and functions. Running code does not have access to local scopeand the global object held within <code>context</code> will be used as the global objectfor <code>code</code>.<code>filename</code> is optional, it&#39;s used only in stack traces.</p><p>Example: compile and execute code in a existing context.</p><pre><code>var util = require(&#39;util&#39;),    vm = require(&#39;vm&#39;),    initSandbox = {      animal: &#39;cat&#39;,      count: 2    },    context = vm.createContext(initSandbox);vm.runInContext(&#39;count += 1; name = &quot;CATT&quot;&#39;, context, &#39;myfile.vm&#39;);console.log(util.inspect(context));// { animal: &#39;cat&#39;, count: 3, name: &#39;CATT&#39; }</code></pre><p>Note that <code>createContext</code> will perform a shallow clone of the supplied sandbox object in order toinitialize the global object of the freshly constructed context.</p><p>Note that running untrusted code is a tricky business requiring great care.  To prevent accidentalglobal variable leakage, <code>vm.runInContext</code> is quite useful, but safely running untrusted coderequires a separate process.</p><p>In case of syntax error in <code>code</code>, <code>vm.runInContext</code> emits the syntax error to stderrand throws an exception.</p></summary>
        /// <signature>
        /// <param name="code"></param>
        /// <param name="context"></param>
        /// <param name="filename"></param>
        /// </signature>
    }
    this.createContext = function(initSandbox) {
        /// <summary><p><code>vm.createContext</code> creates a new context which is suitable for use as the 2nd argument of a subsequentcall to <code>vm.runInContext</code>. A (V8) context comprises a global object together with a set ofbuild-in objects and functions. The optional argument <code>initSandbox</code> will be shallow-copiedto seed the initial contents of the global object used by the context.</p></summary>
        /// <signature>
        /// <param name="initSandbox"></param>
        /// </signature>
        return new vm.Context();
    }
    this.createScript = function(code, filename) {
        /// <summary><p><code>createScript</code> compiles <code>code</code> but does not run it. Instead, it returns a<code>vm.Script</code> object representing this compiled code. This script can be runlater many times using methods below. The returned script is not bound to anyglobal object. It is bound before each run, just for that run. <code>filename</code> isoptional, it&#39;s only used in stack traces.</p><p>In case of syntax error in <code>code</code>, <code>createScript</code> prints the syntax error to stderrand throws an exception.</p></summary>
        /// <signature>
        /// <param name="code"></param>
        /// <param name="filename"></param>
        /// </signature>
        return new vm.Script();
    }
    this.Script = function() {
        this.runInThisContext = function() {
            /// <summary><p>Similar to <code>vm.runInThisContext</code> but a method of a precompiled <code>Script</code> object.<code>script.runInThisContext</code> runs the code of <code>script</code> and returns the result.Running code does not have access to local scope, but does have access to the <code>global</code> object(v8: in actual context).</p><p>Example of using <code>script.runInThisContext</code> to compile code once and run it multiple times:</p><pre><code>var vm = require(&#39;vm&#39;);globalVar = 0;var script = vm.createScript(&#39;globalVar += 1&#39;, &#39;myfile.vm&#39;);for (var i = 0; i &lt; 1000 ; i += 1) {  script.runInThisContext();}console.log(globalVar);// 1000</code></pre></summary>
        }
        this.runInNewContext = function(sandbox) {
            /// <summary><p>Similar to <code>vm.runInNewContext</code> a method of a precompiled <code>Script</code> object.<code>script.runInNewContext</code> runs the code of <code>script</code> with <code>sandbox</code> as the global object and returns the result.Running code does not have access to local scope. <code>sandbox</code> is optional.</p><p>Example: compile code that increments a global variable and sets one, then execute this code multiple times.These globals are contained in the sandbox.</p><pre><code>var util = require(&#39;util&#39;),    vm = require(&#39;vm&#39;),    sandbox = {      animal: &#39;cat&#39;,      count: 2    };var script = vm.createScript(&#39;count += 1; name = &quot;kitty&quot;&#39;, &#39;myfile.vm&#39;);for (var i = 0; i &lt; 10 ; i += 1) {  script.runInNewContext(sandbox);}console.log(util.inspect(sandbox));// { animal: &#39;cat&#39;, count: 12, name: &#39;kitty&#39; }</code></pre><p>Note that running untrusted code is a tricky business requiring great care.  To prevent accidentalglobal variable leakage, <code>script.runInNewContext</code> is quite useful, but safely running untrusted coderequires a separate process.</p></summary>
            /// <signature>
            /// <param name="sandbox"></param>
            /// </signature>
        }
    }
} // module vm

function child_process() {
    /// <summary><p>Node provides a tri-directional <code>popen(3)</code> facility through the<code>child_process</code> module.</p><p>It is possible to stream data through a child&#39;s <code>stdin</code>, <code>stdout</code>, and<code>stderr</code> in a fully non-blocking way.</p><p>To create a child process use <code>require(&#39;child_process&#39;).spawn()</code> or<code>require(&#39;child_process&#39;).fork()</code>.  The semantics of each are slightlydifferent, and explained below.</p></summary>
    this.spawn = function(command, args, options) {
        /// <summary><p>Launches a new process with the given <code>command</code>, with  command line arguments in <code>args</code>.If omitted, <code>args</code> defaults to an empty Array.</p><p>The third argument is used to specify additional options, which defaults to:</p><pre><code>{ cwd: undefined,  env: process.env}</code></pre><p><code>cwd</code> allows you to specify the working directory from which the process is spawned.Use <code>env</code> to specify environment variables that will be visible to the new process.</p><p>Example of running <code>ls -lh /usr</code>, capturing <code>stdout</code>, <code>stderr</code>, and the exit code:</p><pre><code>var spawn = require(&#39;child_process&#39;).spawn,    ls    = spawn(&#39;ls&#39;, [&#39;-lh&#39;, &#39;/usr&#39;]);ls.stdout.on(&#39;data&#39;, function (data) {  console.log(&#39;stdout: &#39; + data);});ls.stderr.on(&#39;data&#39;, function (data) {  console.log(&#39;stderr: &#39; + data);});ls.on(&#39;exit&#39;, function (code) {  console.log(&#39;child process exited with code &#39; + code);});</code></pre><p>Example: A very elaborate way to run &#39;ps ax | grep ssh&#39;</p><pre><code>var spawn = require(&#39;child_process&#39;).spawn,    ps    = spawn(&#39;ps&#39;, [&#39;ax&#39;]),    grep  = spawn(&#39;grep&#39;, [&#39;ssh&#39;]);ps.stdout.on(&#39;data&#39;, function (data) {  grep.stdin.write(data);});ps.stderr.on(&#39;data&#39;, function (data) {  console.log(&#39;ps stderr: &#39; + data);});ps.on(&#39;exit&#39;, function (code) {  if (code !== 0) {    console.log(&#39;ps process exited with code &#39; + code);  }  grep.stdin.end();});grep.stdout.on(&#39;data&#39;, function (data) {  console.log(&#39;&#39; + data);});grep.stderr.on(&#39;data&#39;, function (data) {  console.log(&#39;grep stderr: &#39; + data);});grep.on(&#39;exit&#39;, function (code) {  if (code !== 0) {    console.log(&#39;grep process exited with code &#39; + code);  }});</code></pre><p>Example of checking for failed exec:</p><pre><code>var spawn = require(&#39;child_process&#39;).spawn,    child = spawn(&#39;bad_command&#39;);child.stderr.setEncoding(&#39;utf8&#39;);child.stderr.on(&#39;data&#39;, function (data) {  if (/^execvp\(\)/.test(data)) {    console.log(&#39;Failed to start child process.&#39;);  }});</code></pre><p>Note that if spawn receives an empty options object, it will result inspawning the process with an empty environment rather than using<code>process.env</code>. This due to backwards compatibility issues with a deprecatedAPI.</p><p>The &#39;stdio&#39; option to <code>child_process.spawn()</code> is an array where eachindex corresponds to a fd in the child.  The value is one of the following:</p><ol><li><code>&#39;pipe&#39;</code> - Create a pipe between the child process and the parent process.The parent end of the pipe is exposed to the parent as a property on the<code>child_process</code> object as <code>ChildProcess.stdio[fd]</code>. Pipes created forfds 0 - 2 are also available as ChildProcess.stdin, ChildProcess.stdoutand ChildProcess.stderr, respectively.</li><li><code>&#39;ipc&#39;</code> - Create an IPC channel for passing messages/file descriptorsbetween parent and child. A ChildProcess may have at most <em>one</em> IPC stdiofile descriptor. Setting this option enables the ChildProcess.send() method.If the child writes JSON messages to this file descriptor, then this willtrigger ChildProcess.on(&#39;message&#39;).  If the child is a Node.js program, thenthe presence of an IPC channel will enable process.send() andprocess.on(&#39;message&#39;).</li><li><code>&#39;ignore&#39;</code> - Do not set this file descriptor in the child. Note that Nodewill always open fd 0 - 2 for the processes it spawns. When any of these isignored node will open <code>/dev/null</code> and attach it to the child&#39;s fd.</li><li><code>Stream</code> object - Share a readable or writable stream that refers to a tty,file, socket, or a pipe with the child process. The stream&#39;s underlyingfile descriptor is duplicated in the child process to the fd that corresponds to the index in the <code>stdio</code> array.</li><li>Positive integer - The integer value is interpreted as a file descriptor that is is currently open in the parent process. It is shared with the childprocess, similar to how <code>Stream</code> objects can be shared.</li><li><code>null</code>, <code>undefined</code> - Use default value. For stdio fds 0, 1 and 2 (in otherwords, stdin, stdout, and stderr) a pipe is created. For fd 3 and up, thedefault is <code>&#39;ignore&#39;</code>.</li></ol><p>As a shorthand, the <code>stdio</code> argument may also be one of the followingstrings, rather than an array:</p><ul><li><code>ignore</code> - <code>[&#39;ignore&#39;, &#39;ignore&#39;, &#39;ignore&#39;]</code></li><li><code>pipe</code> - <code>[&#39;pipe&#39;, &#39;pipe&#39;, &#39;pipe&#39;]</code></li><li><code>inherit</code> - <code>[process.stdin, process.stdout, process.stderr]</code> or <code>[0,1,2]</code></li></ul><p>Example:</p><pre><code>var spawn = require(&#39;child_process&#39;).spawn;// Child will use parent&#39;s stdiosspawn(&#39;prg&#39;, [], { stdio: &#39;inherit&#39; });// Spawn child sharing only stderrspawn(&#39;prg&#39;, [], { stdio: [&#39;pipe&#39;, &#39;pipe&#39;, process.stderr] });// Open an extra fd=4, to interact with programs present a// startd-style interface.spawn(&#39;prg&#39;, [], { stdio: [&#39;pipe&#39;, null, null, null, &#39;pipe&#39;] });</code></pre><p>If the <code>detached</code> option is set, the child process will be made the leader of anew process group.  This makes it possible for the child to continue running after the parent exits.</p><p>By default, the parent will wait for the detached child to exit.  To preventthe parent from waiting for a given <code>child</code>, use the <code>child.unref()</code> method,and the parent&#39;s event loop will not include the child in its reference count.</p><p>Example of detaching a long-running process and redirecting its output to afile:</p><pre><code> var fs = require(&#39;fs&#39;),     spawn = require(&#39;child_process&#39;).spawn,     out = fs.openSync(&#39;./out.log&#39;, &#39;a&#39;),     err = fs.openSync(&#39;./out.log&#39;, &#39;a&#39;); var child = spawn(&#39;prg&#39;, [], {   detached: true,   stdio: [ &#39;ignore&#39;, out, err ] }); child.unref();</code></pre><p>When using the <code>detached</code> option to start a long-running process, the processwill not stay running in the background unless it is provided with a <code>stdio</code>configuration that is not connected to the parent.  If the parent&#39;s <code>stdio</code> isinherited, the child will remain attached to the controlling terminal.</p><p>There is a deprecated option called <code>customFds</code> which allows one to specifyspecific file descriptors for the stdio of the child process. This API wasnot portable to all platforms and therefore removed.With <code>customFds</code> it was possible to hook up the new process&#39; <code>[stdin, stdout,stderr]</code> to existing streams; <code>-1</code> meant that a new stream should be created.Use at your own risk.</p><p>There are several internal options. In particular <code>stdinStream</code>,<code>stdoutStream</code>, <code>stderrStream</code>. They are for INTERNAL USE ONLY. As with allundocumented APIs in Node, they should not be used.</p><p>See also: <code>child_process.exec()</code> and <code>child_process.fork()</code></p></summary>
        /// <signature>
        /// <param name="command" type="String">The command to run</param>
        /// <param name="args" type="Array">List of string arguments</param>
        /// <param name="options" type="Object"></param>
        /// <returns type="ChildProcess"></returns>
        /// </signature>
        /// <signature>
        /// <param name="command"></param>
        /// <param name="args"></param>
        /// <param name="options"></param>
        /// </signature>
    }
    this.exec = function(command, options, callback) {
        /// <summary><p>Runs a command in a shell and buffers the output.</p><pre><code>var exec = require(&#39;child_process&#39;).exec,    child;child = exec(&#39;cat *.js bad_file | wc -l&#39;,  function (error, stdout, stderr) {    console.log(&#39;stdout: &#39; + stdout);    console.log(&#39;stderr: &#39; + stderr);    if (error !== null) {      console.log(&#39;exec error: &#39; + error);    }});</code></pre><p>The callback gets the arguments <code>(error, stdout, stderr)</code>. On success, <code>error</code>will be <code>null</code>.  On error, <code>error</code> will be an instance of <code>Error</code> and <code>err.code</code>will be the exit code of the child process, and <code>err.signal</code> will be set to thesignal that terminated the process.</p><p>There is a second optional argument to specify several options. Thedefault options are</p><pre><code>{ encoding: &#39;utf8&#39;,  timeout: 0,  maxBuffer: 200*1024,  killSignal: &#39;SIGTERM&#39;,  cwd: null,  env: null }</code></pre><p>If <code>timeout</code> is greater than 0, then it will kill the child processif it runs longer than <code>timeout</code> milliseconds. The child process is killed with<code>killSignal</code> (default: <code>&#39;SIGTERM&#39;</code>). <code>maxBuffer</code> specifies the largestamount of data allowed on stdout or stderr - if this value is exceeded thenthe child process is killed.</p></summary>
        /// <signature>
        /// <param name="command" type="String">The command to run, with space-separated arguments</param>
        /// <param name="options" type="Object"></param>
        /// <param name="callback" type="Function">called with the output when process terminates</param>
        /// <returns>ChildProcess object</returns>
        /// </signature>
        /// <signature>
        /// <param name="command"></param>
        /// <param name="options"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.execFile = function(file, args, options, callback) {
        /// <summary><p>This is similar to <code>child_process.exec()</code> except it does not execute asubshell but rather the specified file directly. This makes it slightlyleaner than <code>child_process.exec</code>. It has the same options.</p></summary>
        /// <signature>
        /// <param name="file" type="String">The filename of the program to run</param>
        /// <param name="args" type="Array">List of string arguments</param>
        /// <param name="options" type="Object"></param>
        /// <param name="callback" type="Function">called with the output when process terminates</param>
        /// <returns>ChildProcess object</returns>
        /// </signature>
        /// <signature>
        /// <param name="file"></param>
        /// <param name="args"></param>
        /// <param name="options"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.fork = function(modulePath, args, options) {
        /// <summary><p>This is a special case of the <code>spawn()</code> functionality for spawning Nodeprocesses. In addition to having all the methods in a normal ChildProcessinstance, the returned object has a communication channel built-in. See<code>child.send(message, [sendHandle])</code> for details.</p><p>By default the spawned Node process will have the stdout, stderr associatedwith the parent&#39;s. To change this behavior set the <code>silent</code> property in the<code>options</code> object to <code>true</code>.</p><p>The child process does not automatically exit once it&#39;s done, you need to call<code>process.exit()</code> explicitly. This limitation may be lifted in the future.</p><p>These child Nodes are still whole new instances of V8. Assume at least 30msstartup and 10mb memory for each new Node. That is, you cannot create manythousands of them.</p></summary>
        /// <signature>
        /// <param name="modulePath" type="String">The module to run in the child</param>
        /// <param name="args" type="Array">List of string arguments</param>
        /// <param name="options" type="Object"></param>
        /// <returns>ChildProcess object</returns>
        /// </signature>
        /// <signature>
        /// <param name="modulePath"></param>
        /// <param name="args"></param>
        /// <param name="options"></param>
        /// </signature>
    }
    this.ChildProcess = function() {
        this.kill = function(signal) {
            /// <summary><p>Send a signal to the child process. If no argument is given, the process willbe sent <code>&#39;SIGTERM&#39;</code>. See <code>signal(7)</code> for a list of available signals.</p><pre><code>var spawn = require(&#39;child_process&#39;).spawn,    grep  = spawn(&#39;grep&#39;, [&#39;ssh&#39;]);grep.on(&#39;exit&#39;, function (code, signal) {  console.log(&#39;child process terminated due to receipt of signal &#39;+signal);});// send SIGHUP to processgrep.kill(&#39;SIGHUP&#39;);</code></pre><p>Note that while the function is called <code>kill</code>, the signal delivered to the childprocess may not actually kill it.  <code>kill</code> really just sends a signal to a process.</p><p>See <code>kill(2)</code></p></summary>
            /// <signature>
            /// <param name="signal" type="String"></param>
            /// </signature>
            /// <signature>
            /// <param name="signal"></param>
            /// </signature>
        }
        this.send = function(message, sendHandle) {
            /// <summary><p>When using <code>child_process.fork()</code> you can write to the child using<code>child.send(message, [sendHandle])</code> and messages are received bya <code>&#39;message&#39;</code> event on the child.</p><p>For example:</p><pre><code>var cp = require(&#39;child_process&#39;);var n = cp.fork(__dirname + &#39;/sub.js&#39;);n.on(&#39;message&#39;, function(m) {  console.log(&#39;PARENT got message:&#39;, m);});n.send({ hello: &#39;world&#39; });</code></pre><p>And then the child script, <code>&#39;sub.js&#39;</code> might look like this:</p><pre><code>process.on(&#39;message&#39;, function(m) {  console.log(&#39;CHILD got message:&#39;, m);});process.send({ foo: &#39;bar&#39; });</code></pre><p>In the child the <code>process</code> object will have a <code>send()</code> method, and <code>process</code>will emit objects each time it receives a message on its channel.</p><p>There is a special case when sending a <code>{cmd: &#39;NODE_foo&#39;}</code> message. All messagescontaining a <code>NODE_</code> prefix in its <code>cmd</code> property will not be emitted inthe <code>message</code> event, since they are internal messages used by node core.Messages containing the prefix are emitted in the <code>internalMessage</code> event, youshould by all means avoid using this feature, it is subject to change without notice.</p><p>The <code>sendHandle</code> option to <code>child.send()</code> is for sending a TCP server orsocket object to another process. The child will receive the object as itssecond argument to the <code>message</code> event.</p><p><strong>send server object</strong></p><p>Here is an example of sending a server:</p><pre><code>var child = require(&#39;child_process&#39;).fork(&#39;child.js&#39;);// Open up the server object and send the handle.var server = require(&#39;net&#39;).createServer();server.on(&#39;connection&#39;, function (socket) {  socket.end(&#39;handled by parent&#39;);});server.listen(1337, function() {  child.send(&#39;server&#39;, server);});</code></pre><p>And the child would the receive the server object as:</p><pre><code>process.on(&#39;message&#39;, function(m, server) {  if (m === &#39;server&#39;) {    server.on(&#39;connection&#39;, function (socket) {      socket.end(&#39;handled by child&#39;);    });  }});</code></pre><p>Note that the server is now shared between the parent and child, this meansthat some connections will be handled by the parent and some by the child.</p><p><strong>send socket object</strong></p><p>Here is an example of sending a socket. It will spawn two children and handleconnections with the remote address <code>74.125.127.100</code> as VIP by sending thesocket to a &quot;special&quot; child process. Other sockets will go to a &quot;normal&quot; process.</p><pre><code>var normal = require(&#39;child_process&#39;).fork(&#39;child.js&#39;, [&#39;normal&#39;]);var special = require(&#39;child_process&#39;).fork(&#39;child.js&#39;, [&#39;special&#39;]);// Open up the server and send sockets to childvar server = require(&#39;net&#39;).createServer();server.on(&#39;connection&#39;, function (socket) {  // if this is a VIP  if (socket.remoteAddress === &#39;74.125.127.100&#39;) {    special.send(&#39;socket&#39;, socket);    return;  }  // just the usual dudes  normal.send(&#39;socket&#39;, socket);});server.listen(1337);</code></pre><p>The <code>child.js</code> could look like this:</p><pre><code>process.on(&#39;message&#39;, function(m, socket) {  if (m === &#39;socket&#39;) {    socket.end(&#39;You were handled as a &#39; + process.argv[2] + &#39; person&#39;);  }});</code></pre><p>Note that once a single socket has been sent to a child the parent can nolonger keep track of when the socket is destroyed. To indicate this conditionthe <code>.connections</code> property becomes <code>null</code>.It is also recommended not to use <code>.maxConnections</code> in this condition.</p></summary>
            /// <signature>
            /// <param name="message" type="Object"></param>
            /// <param name="sendHandle" type="Handle"></param>
            /// </signature>
            /// <signature>
            /// <param name="message"></param>
            /// <param name="sendHandle"></param>
            /// </signature>
        }
        this.disconnect = function() {
            /// <summary><p>To close the IPC connection between parent and child use the<code>child.disconnect()</code> method. This allows the child to exit gracefully sincethere is no IPC channel keeping it alive. When calling this method the<code>disconnect</code> event will be emitted in both parent and child, and the<code>connected</code> flag will be set to <code>false</code>. Please note that you can also call<code>process.disconnect()</code> in the child process.</p></summary>
        }
        emitter = new Events().EventEmitter;
        /// <field name='exit'><p>This event is emitted after the child process ends. If the process terminatednormally, <code>code</code> is the final exit code of the process, otherwise <code>null</code>. Ifthe process terminated due to receipt of a signal, <code>signal</code> is the string nameof the signal, otherwise <code>null</code>.</p><p>Note that the child process stdio streams might still be open.</p><p>See <code>waitpid(2)</code>.</p></field>
        this.exit = new emitter();
        /// <field name='close'><p>This event is emitted when the stdio streams of a child process have allterminated.  This is distinct from &#39;exit&#39;, since multiple processesmight share the same stdio streams.</p></field>
        this.close = new emitter();
        /// <field name='disconnect'><p>This event is emitted after using the <code>.disconnect()</code> method in the parent orin the child. After disconnecting it is no longer possible to send messages.An alternative way to check if you can send messages is to see if the<code>child.connected</code> property is <code>true</code>.</p></field>
        this.disconnect = new emitter();
        /// <field name='message'><p>Messages send by <code>.send(message, [sendHandle])</code> are obtained using the<code>message</code> event.</p></field>
        this.message = new emitter();
        /// <field name='stdin'><p>A <code>Writable Stream</code> that represents the child process&#39;s <code>stdin</code>.Closing this stream via <code>end()</code> often causes the child process to terminate.</p><p>If the child stdio streams are shared with the parent, then this willnot be set.</p></field>
        this.stdin = undefined;
        /// <field name='stdout'><p>A <code>Readable Stream</code> that represents the child process&#39;s <code>stdout</code>.</p><p>If the child stdio streams are shared with the parent, then this willnot be set.</p></field>
        this.stdout = undefined;
        /// <field name='stderr'><p>A <code>Readable Stream</code> that represents the child process&#39;s <code>stderr</code>.</p><p>If the child stdio streams are shared with the parent, then this willnot be set.</p></field>
        this.stderr = undefined;
        /// <field name='pid'><p>The PID of the child process.</p><p>Example:</p><pre><code>var spawn = require(&#39;child_process&#39;).spawn,    grep  = spawn(&#39;grep&#39;, [&#39;ssh&#39;]);console.log(&#39;Spawned child pid: &#39; + grep.pid);grep.stdin.end();</code></pre></field>
        this.pid = undefined;
    }
} // module child_process

function assert() {
    /// <summary><p>This module is used for writing unit tests for your applications, you canaccess it with <code>require(&#39;assert&#39;)</code>.</p></summary>
    this.fail = function(actual, expected, message, operator) {
        /// <summary><p>Throws an exception that displays the values for <code>actual</code> and <code>expected</code> separated by the provided operator.</p></summary>
        /// <signature>
        /// <param name="actual"></param>
        /// <param name="expected"></param>
        /// <param name="message"></param>
        /// <param name="operator"></param>
        /// </signature>
    }
    this.ok = function(value, message, assert, message) {
        /// <summary><p>Tests if value is truthy, it is equivalent to <code>assert.equal(true, !!value, message);</code></p></summary>
        /// <signature>
        /// <param name="value"></param>
        /// <param name="message)"></param>
        /// <param name="assert.ok(value"></param>
        /// <param name="message"></param>
        /// </signature>
    }
    this.equal = function(actual, expected, message) {
        /// <summary><p>Tests shallow, coercive equality with the equal comparison operator ( <code>==</code> ).</p></summary>
        /// <signature>
        /// <param name="actual"></param>
        /// <param name="expected"></param>
        /// <param name="message"></param>
        /// </signature>
    }
    this.notEqual = function(actual, expected, message) {
        /// <summary><p>Tests shallow, coercive non-equality with the not equal comparison operator ( <code>!=</code> ).</p></summary>
        /// <signature>
        /// <param name="actual"></param>
        /// <param name="expected"></param>
        /// <param name="message"></param>
        /// </signature>
    }
    this.deepEqual = function(actual, expected, message) {
        /// <summary><p>Tests for deep equality.</p></summary>
        /// <signature>
        /// <param name="actual"></param>
        /// <param name="expected"></param>
        /// <param name="message"></param>
        /// </signature>
    }
    this.notDeepEqual = function(actual, expected, message) {
        /// <summary><p>Tests for any deep inequality.</p></summary>
        /// <signature>
        /// <param name="actual"></param>
        /// <param name="expected"></param>
        /// <param name="message"></param>
        /// </signature>
    }
    this.strictEqual = function(actual, expected, message) {
        /// <summary><p>Tests strict equality, as determined by the strict equality operator ( <code>===</code> )</p></summary>
        /// <signature>
        /// <param name="actual"></param>
        /// <param name="expected"></param>
        /// <param name="message"></param>
        /// </signature>
    }
    this.notStrictEqual = function(actual, expected, message) {
        /// <summary><p>Tests strict non-equality, as determined by the strict not equal operator ( <code>!==</code> )</p></summary>
        /// <signature>
        /// <param name="actual"></param>
        /// <param name="expected"></param>
        /// <param name="message"></param>
        /// </signature>
    }
    this.throws = function(block, error, message) {
        /// <summary><p>Expects <code>block</code> to throw an error. <code>error</code> can be constructor, regexp or validation function.</p><p>Validate instanceof using constructor:</p><pre><code>assert.throws(  function() {    throw new Error(&quot;Wrong value&quot;);  },  Error);</code></pre><p>Validate error message using RegExp:</p><pre><code>assert.throws(  function() {    throw new Error(&quot;Wrong value&quot;);  },  /value/);</code></pre><p>Custom error validation:</p><pre><code>assert.throws(  function() {    throw new Error(&quot;Wrong value&quot;);  },  function(err) {    if ( (err instanceof Error) &amp;&amp; /value/.test(err) ) {      return true;    }  },  &quot;unexpected error&quot;);</code></pre></summary>
        /// <signature>
        /// <param name="block"></param>
        /// <param name="error"></param>
        /// <param name="message"></param>
        /// </signature>
    }
    this.doesNotThrow = function(block, error, message) {
        /// <summary><p>Expects <code>block</code> not to throw an error, see assert.throws for details.</p></summary>
        /// <signature>
        /// <param name="block"></param>
        /// <param name="error"></param>
        /// <param name="message"></param>
        /// </signature>
    }
    this.ifError = function(value) {
        /// <summary><p>Tests if value is not a false value, throws if it is a true value. Useful whentesting the first argument, <code>error</code> in callbacks.</p></summary>
        /// <signature>
        /// <param name="value"></param>
        /// </signature>
    }
} // module assert

function tty() {
    /// <summary><p>The <code>tty</code> module houses the <code>tty.ReadStream</code> and <code>tty.WriteStream</code> classes. Inmost cases, you will not need to use this module directly.</p><p>When node detects that it is being run inside a TTY context, then <code>process.stdin</code>will be a <code>tty.ReadStream</code> instance and <code>process.stdout</code> will bea <code>tty.WriteStream</code> instance. The preferred way to check if node is being run ina TTY context is to check <code>process.stdout.isTTY</code>:</p><pre><code>$ node -p -e &quot;Boolean(process.stdout.isTTY)&quot;true$ node -p -e &quot;Boolean(process.stdout.isTTY)&quot; | catfalse</code></pre></summary>
    this.isatty = function(fd) {
        /// <summary><p>Returns <code>true</code> or <code>false</code> depending on if the <code>fd</code> is associated with aterminal.</p></summary>
        /// <signature>
        /// <param name="fd"></param>
        /// </signature>
    }
    this.setRawMode = function(mode) {
        /// <summary><p>Deprecated. Use <code>tty.ReadStream#setRawMode()</code>(i.e. <code>process.stdin.setRawMode()</code>) instead.</p></summary>
        /// <signature>
        /// <param name="mode"></param>
        /// </signature>
    }
    this.ReadStream = function() {
        this.setRawMode = function(mode) {
            /// <summary><p><code>mode</code> should be <code>true</code> or <code>false</code>. This sets the properties of the<code>tty.ReadStream</code> to act either as a raw device or default. <code>isRaw</code> will be setto the resulting mode.</p></summary>
            /// <signature>
            /// <param name="mode"></param>
            /// </signature>
        }
        /// <field name='isRaw'><p>A <code>Boolean</code> that is initialized to <code>false</code>. It represents the current &quot;raw&quot; stateof the <code>tty.ReadStream</code> instance.</p></field>
        this.isRaw = true;
    }
} // module tty

function zlib() {
    /// <summary><p>You can access this module with:</p><pre><code>var zlib = require(&#39;zlib&#39;);</code></pre><p>This provides bindings to Gzip/Gunzip, Deflate/Inflate, andDeflateRaw/InflateRaw classes.  Each class takes the same options, andis a readable/writable Stream.</p><h2>Examples</h2><p>Compressing or decompressing a file can be done by piping anfs.ReadStream into a zlib stream, then into an fs.WriteStream.</p><pre><code>var gzip = zlib.createGzip();var fs = require(&#39;fs&#39;);var inp = fs.createReadStream(&#39;input.txt&#39;);var out = fs.createWriteStream(&#39;input.txt.gz&#39;);inp.pipe(gzip).pipe(out);</code></pre><p>Compressing or decompressing data in one step can be done by usingthe convenience methods.</p><pre><code>var input = &#39;.................................&#39;;zlib.deflate(input, function(err, buffer) {  if (!err) {    console.log(buffer.toString(&#39;base64&#39;));  }});var buffer = new Buffer(&#39;eJzT0yMAAGTvBe8=&#39;, &#39;base64&#39;);zlib.unzip(buffer, function(err, buffer) {  if (!err) {    console.log(buffer.toString());  }});</code></pre><p>To use this module in an HTTP client or server, use the<a href="http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.3">accept-encoding</a>on requests, and the<a href="http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.11">content-encoding</a>header on responses.</p><p><strong>Note: these examples are drastically simplified to showthe basic concept.</strong>  Zlib encoding can be expensive, and the resultsought to be cached.  See <a href="#zlib_memory_usage_tuning">Memory Usage Tuning</a>below for more information on the speed/memory/compressiontradeoffs involved in zlib usage.</p><pre><code>// client request examplevar zlib = require(&#39;zlib&#39;);var http = require(&#39;http&#39;);var fs = require(&#39;fs&#39;);var request = http.get({ host: &#39;izs.me&#39;,                         path: &#39;/&#39;,                         port: 80,                         headers: { &#39;accept-encoding&#39;: &#39;gzip,deflate&#39; } });request.on(&#39;response&#39;, function(response) {  var output = fs.createWriteStream(&#39;izs.me_index.html&#39;);  switch (response.headers[&#39;content-encoding&#39;]) {    // or, just use zlib.createUnzip() to handle both cases    case &#39;gzip&#39;:      response.pipe(zlib.createGunzip()).pipe(output);      break;    case &#39;deflate&#39;:      response.pipe(zlib.createInflate()).pipe(output);      break;    default:      response.pipe(output);      break;  }});// server example// Running a gzip operation on every request is quite expensive.// It would be much more efficient to cache the compressed buffer.var zlib = require(&#39;zlib&#39;);var http = require(&#39;http&#39;);var fs = require(&#39;fs&#39;);http.createServer(function(request, response) {  var raw = fs.createReadStream(&#39;index.html&#39;);  var acceptEncoding = request.headers[&#39;accept-encoding&#39;];  if (!acceptEncoding) {    acceptEncoding = &#39;&#39;;  }  // Note: this is not a conformant accept-encoding parser.  // See http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.3  if (acceptEncoding.match(/\bdeflate\b/)) {    response.writeHead(200, { &#39;content-encoding&#39;: &#39;deflate&#39; });    raw.pipe(zlib.createDeflate()).pipe(response);  } else if (acceptEncoding.match(/\bgzip\b/)) {    response.writeHead(200, { &#39;content-encoding&#39;: &#39;gzip&#39; });    raw.pipe(zlib.createGzip()).pipe(response);  } else {    response.writeHead(200, {});    raw.pipe(response);  }}).listen(1337);</code></pre></summary>
    this.createGzip = function(options) {
        /// <summary><p>Returns a new <a href="#zlib_class_zlib_gzip">Gzip</a> object with an<a href="#zlib_options">options</a>.</p></summary>
        /// <signature>
        /// <param name="options"></param>
        /// </signature>
        return new zlib.Gzip();
    }
    this.createGunzip = function(options) {
        /// <summary><p>Returns a new <a href="#zlib_class_zlib_gunzip">Gunzip</a> object with an<a href="#zlib_options">options</a>.</p></summary>
        /// <signature>
        /// <param name="options"></param>
        /// </signature>
        return new zlib.Gunzip();
    }
    this.createDeflate = function(options) {
        /// <summary><p>Returns a new <a href="#zlib_class_zlib_deflate">Deflate</a> object with an<a href="#zlib_options">options</a>.</p></summary>
        /// <signature>
        /// <param name="options"></param>
        /// </signature>
        return new zlib.Deflate();
    }
    this.createInflate = function(options) {
        /// <summary><p>Returns a new <a href="#zlib_class_zlib_inflate">Inflate</a> object with an<a href="#zlib_options">options</a>.</p></summary>
        /// <signature>
        /// <param name="options"></param>
        /// </signature>
        return new zlib.Inflate();
    }
    this.createDeflateRaw = function(options) {
        /// <summary><p>Returns a new <a href="#zlib_class_zlib_deflateraw">DeflateRaw</a> object with an<a href="#zlib_options">options</a>.</p></summary>
        /// <signature>
        /// <param name="options"></param>
        /// </signature>
        return new zlib.DeflateRaw();
    }
    this.createInflateRaw = function(options) {
        /// <summary><p>Returns a new <a href="#zlib_class_zlib_inflateraw">InflateRaw</a> object with an<a href="#zlib_options">options</a>.</p></summary>
        /// <signature>
        /// <param name="options"></param>
        /// </signature>
        return new zlib.InflateRaw();
    }
    this.createUnzip = function(options) {
        /// <summary><p>Returns a new <a href="#zlib_class_zlib_unzip">Unzip</a> object with an<a href="#zlib_options">options</a>.</p></summary>
        /// <signature>
        /// <param name="options"></param>
        /// </signature>
        return new zlib.Unzip();
    }
    this.deflate = function(buf, callback) {
        /// <summary><p>Compress a string with Deflate.</p></summary>
        /// <signature>
        /// <param name="buf"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.deflateRaw = function(buf, callback) {
        /// <summary><p>Compress a string with DeflateRaw.</p></summary>
        /// <signature>
        /// <param name="buf"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.gzip = function(buf, callback) {
        /// <summary><p>Compress a string with Gzip.</p></summary>
        /// <signature>
        /// <param name="buf"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.gunzip = function(buf, callback) {
        /// <summary><p>Decompress a raw Buffer with Gunzip.</p></summary>
        /// <signature>
        /// <param name="buf"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.inflate = function(buf, callback) {
        /// <summary><p>Decompress a raw Buffer with Inflate.</p></summary>
        /// <signature>
        /// <param name="buf"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.inflateRaw = function(buf, callback) {
        /// <summary><p>Decompress a raw Buffer with InflateRaw.</p></summary>
        /// <signature>
        /// <param name="buf"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.unzip = function(buf, callback) {
        /// <summary><p>Decompress a raw Buffer with Unzip.</p></summary>
        /// <signature>
        /// <param name="buf"></param>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.Gzip = function() {
    }
    this.Gunzip = function() {
    }
    this.Deflate = function() {
    }
    this.Inflate = function() {
    }
    this.DeflateRaw = function() {
    }
    this.InflateRaw = function() {
    }
    this.Unzip = function() {
    }
} // module zlib

function os() {
    /// <summary><p>Provides a few basic operating-system related utility functions.</p><p>Use <code>require(&#39;os&#39;)</code> to access this module.</p></summary>
    this.tmpDir = function() {
        /// <summary><p>Returns the operating system&#39;s default directory for temp files.</p></summary>
    }
    this.hostname = function() {
        /// <summary><p>Returns the hostname of the operating system.</p></summary>
    }
    this.type = function() {
        /// <summary><p>Returns the operating system name.</p></summary>
    }
    this.platform = function() {
        /// <summary><p>Returns the operating system platform.</p></summary>
    }
    this.arch = function() {
        /// <summary><p>Returns the operating system CPU architecture.</p></summary>
    }
    this.release = function() {
        /// <summary><p>Returns the operating system release.</p></summary>
    }
    this.uptime = function() {
        /// <summary><p>Returns the system uptime in seconds.</p></summary>
    }
    this.loadavg = function() {
        /// <summary><p>Returns an array containing the 1, 5, and 15 minute load averages.</p></summary>
    }
    this.totalmem = function() {
        /// <summary><p>Returns the total amount of system memory in bytes.</p></summary>
    }
    this.freemem = function() {
        /// <summary><p>Returns the amount of free system memory in bytes.</p></summary>
    }
    this.cpus = function() {
        /// <summary><p>Returns an array of objects containing information about each CPU/core installed: model, speed (in MHz), and times (an object containing the number of CPU ticks spent in: user, nice, sys, idle, and irq).</p><p>Example inspection of os.cpus:</p><pre><code>[ { model: &#39;Intel(R) Core(TM) i7 CPU         860  @ 2.80GHz&#39;,    speed: 2926,    times:     { user: 252020,       nice: 0,       sys: 30340,       idle: 1070356870,       irq: 0 } },  { model: &#39;Intel(R) Core(TM) i7 CPU         860  @ 2.80GHz&#39;,    speed: 2926,    times:     { user: 306960,       nice: 0,       sys: 26980,       idle: 1071569080,       irq: 0 } },  { model: &#39;Intel(R) Core(TM) i7 CPU         860  @ 2.80GHz&#39;,    speed: 2926,    times:     { user: 248450,       nice: 0,       sys: 21750,       idle: 1070919370,       irq: 0 } },  { model: &#39;Intel(R) Core(TM) i7 CPU         860  @ 2.80GHz&#39;,    speed: 2926,    times:     { user: 256880,       nice: 0,       sys: 19430,       idle: 1070905480,       irq: 20 } },  { model: &#39;Intel(R) Core(TM) i7 CPU         860  @ 2.80GHz&#39;,    speed: 2926,    times:     { user: 511580,       nice: 20,       sys: 40900,       idle: 1070842510,       irq: 0 } },  { model: &#39;Intel(R) Core(TM) i7 CPU         860  @ 2.80GHz&#39;,    speed: 2926,    times:     { user: 291660,       nice: 0,       sys: 34360,       idle: 1070888000,       irq: 10 } },  { model: &#39;Intel(R) Core(TM) i7 CPU         860  @ 2.80GHz&#39;,    speed: 2926,    times:     { user: 308260,       nice: 0,       sys: 55410,       idle: 1071129970,       irq: 880 } },  { model: &#39;Intel(R) Core(TM) i7 CPU         860  @ 2.80GHz&#39;,    speed: 2926,    times:     { user: 266450,       nice: 1480,       sys: 34920,       idle: 1072572010,       irq: 30 } } ]</code></pre></summary>
    }
    this.networkInterfaces = function() {
        /// <summary><p>Get a list of network interfaces:</p><pre><code>{ lo0:    [ { address: &#39;::1&#39;, family: &#39;IPv6&#39;, internal: true },     { address: &#39;fe80::1&#39;, family: &#39;IPv6&#39;, internal: true },     { address: &#39;127.0.0.1&#39;, family: &#39;IPv4&#39;, internal: true } ],  en1:    [ { address: &#39;fe80::cabc:c8ff:feef:f996&#39;, family: &#39;IPv6&#39;,       internal: false },     { address: &#39;10.0.1.123&#39;, family: &#39;IPv4&#39;, internal: false } ],  vmnet1: [ { address: &#39;10.99.99.254&#39;, family: &#39;IPv4&#39;, internal: false } ],  vmnet8: [ { address: &#39;10.88.88.1&#39;, family: &#39;IPv4&#39;, internal: false } ],  ppp0: [ { address: &#39;10.2.0.231&#39;, family: &#39;IPv4&#39;, internal: false } ] }</code></pre></summary>
    }
    /// <field name='EOL'><p>A constant defining the appropriate End-of-line marker for the operating system.</p></field>
    this.EOL = undefined;
} // module os

function cluster() {
    /// <summary><p>A single instance of Node runs in a single thread. To take advantage ofmulti-core systems the user will sometimes want to launch a cluster of Nodeprocesses to handle the load.</p><p>The cluster module allows you to easily create a network of processes thatall share server ports.</p><pre><code>var cluster = require(&#39;cluster&#39;);var http = require(&#39;http&#39;);var numCPUs = require(&#39;os&#39;).cpus().length;if (cluster.isMaster) {  // Fork workers.  for (var i = 0; i &lt; numCPUs; i++) {    cluster.fork();  }  cluster.on(&#39;exit&#39;, function(worker, code, signal) {    console.log(&#39;worker &#39; + worker.process.pid + &#39; died&#39;);  });} else {  // Workers can share any TCP connection  // In this case its a HTTP server  http.createServer(function(req, res) {    res.writeHead(200);    res.end(&quot;hello world\n&quot;);  }).listen(8000);}</code></pre><p>Running node will now share port 8000 between the workers:</p><pre><code>% node server.jsWorker 2438 onlineWorker 2437 online</code></pre><p>This feature was introduced recently, and may change in future versions.Please try it out and provide feedback.</p><p>Also note that, on Windows, it is not yet possible to set up a named pipeserver in a worker.</p></summary>
    this.setupMaster = function(settings) {
        /// <summary><p><code>setupMaster</code> is used to change the default &#39;fork&#39; behavior. The new settingsare effective immediately and permanently, they cannot be changed later on.</p><p>Example:</p><pre><code>var cluster = require(&quot;cluster&quot;);cluster.setupMaster({  exec : &quot;worker.js&quot;,  args : [&quot;--use&quot;, &quot;https&quot;],  silent : true});cluster.fork();</code></pre></summary>
        /// <signature>
        /// <param name="settings" type="Object"></param>
        /// </signature>
        /// <signature>
        /// <param name="settings"></param>
        /// </signature>
    }
    this.fork = function(env) {
        /// <summary><p>Spawn a new worker process. This can only be called from the master process.</p></summary>
        /// <signature>
        /// <param name="env" type="Object">Key/value pairs to add to child process environment.</param>
        /// <returns type="Worker"></returns>
        /// </signature>
        /// <signature>
        /// <param name="env"></param>
        /// </signature>
    }
    this.disconnect = function(callback) {
        /// <summary><p>When calling this method, all workers will commit a graceful suicide. When they aredisconnected all internal handlers will be closed, allowing the master process todie graceful if no other event is waiting.</p><p>The method takes an optional callback argument which will be called when finished.</p></summary>
        /// <signature>
        /// <param name="callback" type="Function">called when all workers are disconnected and handlers are closed</param>
        /// </signature>
        /// <signature>
        /// <param name="callback"></param>
        /// </signature>
    }
    emitter = new Events().EventEmitter;
    /// <field name='fork'><p>When a new worker is forked the cluster module will emit a &#39;fork&#39; event.This can be used to log worker activity, and create you own timeout.</p><pre><code>var timeouts = [];function errorMsg() {  console.error(&quot;Something must be wrong with the connection ...&quot;);}cluster.on(&#39;fork&#39;, function(worker) {  timeouts[worker.id] = setTimeout(errorMsg, 2000);});cluster.on(&#39;listening&#39;, function(worker, address) {  clearTimeout(timeouts[worker.id]);});cluster.on(&#39;exit&#39;, function(worker, code, signal) {  clearTimeout(timeouts[worker.id]);  errorMsg();});</code></pre></field>
    this.fork = new emitter();
    /// <field name='online'><p>After forking a new worker, the worker should respond with a online message.When the master receives a online message it will emit such event.The difference between &#39;fork&#39; and &#39;online&#39; is that fork is emitted when themaster tries to fork a worker, and &#39;online&#39; is emitted when the worker isbeing executed.</p><pre><code>cluster.on(&#39;online&#39;, function(worker) {  console.log(&quot;Yay, the worker responded after it was forked&quot;);});</code></pre></field>
    this.online = new emitter();
    /// <field name='listening'><p>When calling <code>listen()</code> from a worker, a &#39;listening&#39; event is automatically assignedto the server instance. When the server is listening a message is send to the masterwhere the &#39;listening&#39; event is emitted.</p><p>The event handler is executed with two arguments, the <code>worker</code> contains the workerobject and the <code>address</code> object contains the following connection properties:<code>address</code>, <code>port</code> and <code>addressType</code>. This is very useful if the worker is listeningon more than one address.</p><pre><code>cluster.on(&#39;listening&#39;, function(worker, address) {  console.log(&quot;A worker is now connected to &quot; + address.address + &quot;:&quot; + address.port);});</code></pre></field>
    this.listening = new emitter();
    /// <field name='disconnect'><p>When a workers IPC channel has disconnected this event is emitted. This will happenwhen the worker dies, usually after calling <code>.destroy()</code>.</p><p>When calling <code>.disconnect()</code>, there may be a delay between the<code>disconnect</code> and <code>exit</code> events.  This event can be used to detect ifthe process is stuck in a cleanup or if there are long-livingconnections.</p><pre><code>cluster.on(&#39;disconnect&#39;, function(worker) {  console.log(&#39;The worker #&#39; + worker.id + &#39; has disconnected&#39;);});</code></pre></field>
    this.disconnect = new emitter();
    /// <field name='exit'><p>When any of the workers die the cluster module will emit the &#39;exit&#39; event.This can be used to restart the worker by calling <code>fork()</code> again.</p><pre><code>cluster.on(&#39;exit&#39;, function(worker, code, signal) {  var exitCode = worker.process.exitCode;  console.log(&#39;worker &#39; + worker.process.pid + &#39; died (&#39;+exitCode+&#39;). restarting...&#39;);  cluster.fork();});</code></pre></field>
    this.exit = new emitter();
    /// <field name='setup'><p>When the <code>.setupMaster()</code> function has been executed this event emits.If <code>.setupMaster()</code> was not executed before <code>fork()</code> this function willcall <code>.setupMaster()</code> with no arguments.</p></field>
    this.setup = new emitter();
    this.Worker = function() {
        this.send = function(message, sendHandle) {
            /// <summary><p>This function is equal to the send methods provided by<code>child_process.fork()</code>.  In the master you should use this function tosend a message to a specific worker.  However in a worker you can also use<code>process.send(message)</code>, since this is the same function.</p><p>This example will echo back all messages from the master:</p><pre><code>if (cluster.isMaster) {  var worker = cluster.fork();  worker.send(&#39;hi there&#39;);} else if (cluster.isWorker) {  process.on(&#39;message&#39;, function(msg) {    process.send(msg);  });}</code></pre></summary>
            /// <signature>
            /// <param name="message" type="Object"></param>
            /// <param name="sendHandle" type="Handle"></param>
            /// </signature>
            /// <signature>
            /// <param name="message"></param>
            /// <param name="sendHandle"></param>
            /// </signature>
        }
        this.destroy = function() {
            /// <summary><p>This function will kill the worker, and inform the master to not spawn anew worker.  The boolean <code>suicide</code> lets you distinguish between voluntaryand accidental exit.</p><pre><code>cluster.on(&#39;exit&#39;, function(worker, code, signal) {  if (worker.suicide === true) {    console.log(&#39;Oh, it was just suicide\&#39; â€“ no need to worry&#39;).  }});// destroy workerworker.destroy();</code></pre></summary>
        }
        this.disconnect = function() {
            /// <summary><p>When calling this function the worker will no longer accept new connections, butthey will be handled by any other listening worker. Existing connection will beallowed to exit as usual. When no more connections exist, the IPC channel to the workerwill close allowing it to die graceful. When the IPC channel is closed the <code>disconnect</code>event will emit, this is then followed by the <code>exit</code> event, there is emitted whenthe worker finally die.</p><p>Because there might be long living connections, it is useful to implement a timeout.This example ask the worker to disconnect and after 2 seconds it will destroy theserver. An alternative would be to execute <code>worker.destroy()</code> after 2 seconds, butthat would normally not allow the worker to do any cleanup if needed.</p><pre><code>if (cluster.isMaster) {  var worker = cluster.fork();  var timeout;  worker.on(&#39;listening&#39;, function(address) {    worker.disconnect();    timeout = setTimeout(function() {      worker.send(&#39;force kill&#39;);    }, 2000);  });  worker.on(&#39;disconnect&#39;, function() {    clearTimeout(timeout);  });} else if (cluster.isWorker) {  var net = require(&#39;net&#39;);  var server = net.createServer(function(socket) {    // connection never end  });  server.listen(8000);  server.on(&#39;close&#39;, function() {    // cleanup  });  process.on(&#39;message&#39;, function(msg) {    if (msg === &#39;force kill&#39;) {      server.destroy();    }  });}</code></pre></summary>
        }
        emitter = new Events().EventEmitter;
        /// <field name='message'><p>This event is the same as the one provided by <code>child_process.fork()</code>.In the master you should use this event, however in a worker you can also use<code>process.on(&#39;message&#39;)</code></p><p>As an example, here is a cluster that keeps count of the number of requestsin the master process using the message system:</p><pre><code>var cluster = require(&#39;cluster&#39;);var http = require(&#39;http&#39;);if (cluster.isMaster) {  // Keep track of http requests  var numReqs = 0;  setInterval(function() {    console.log(&quot;numReqs =&quot;, numReqs);  }, 1000);  // Count requestes  function messageHandler(msg) {    if (msg.cmd &amp;&amp; msg.cmd == &#39;notifyRequest&#39;) {      numReqs += 1;    }  }  // Start workers and listen for messages containing notifyRequest  var numCPUs = require(&#39;os&#39;).cpus().length;  for (var i = 0; i &lt; numCPUs; i++) {    cluster.fork();  }  Object.keys(cluster.workers).forEach(function(id) {    cluster.workers[id].on(&#39;message&#39;, messageHandler);  });} else {  // Worker processes have a http server.  http.Server(function(req, res) {    res.writeHead(200);    res.end(&quot;hello world\n&quot;);    // notify master about the request    process.send({ cmd: &#39;notifyRequest&#39; });  }).listen(8000);}</code></pre></field>
        this.message = new emitter();
        /// <field name='online'><p>Same as the <code>cluster.on(&#39;online&#39;)</code> event, but emits only when the state changeon the specified worker.</p><pre><code>cluster.fork().on(&#39;online&#39;, function() {  // Worker is online};</code></pre></field>
        this.online = new emitter();
        /// <field name='listening'><p>Same as the <code>cluster.on(&#39;listening&#39;)</code> event, but emits only when the state changeon the specified worker.</p><pre><code>cluster.fork().on(&#39;listening&#39;, function(address) {  // Worker is listening};</code></pre></field>
        this.listening = new emitter();
        /// <field name='disconnect'><p>Same as the <code>cluster.on(&#39;disconnect&#39;)</code> event, but emits only when the state changeon the specified worker.</p><pre><code>cluster.fork().on(&#39;disconnect&#39;, function() {  // Worker has disconnected};</code></pre></field>
        this.disconnect = new emitter();
        /// <field name='exit'><p>Emitted by the individual worker instance, when the underlying child processis terminated.  See <a href="child_process.html#child_process_event_exit">child_process event: &#39;exit&#39;</a>.</p><pre><code>var worker = cluster.fork();worker.on(&#39;exit&#39;, function(code, signal) {  if( signal ) {    console.log(&quot;worker was killed by signal: &quot;+signal);  } else if( code !== 0 ) {    console.log(&quot;worker exited with error code: &quot;+code);  } else {    console.log(&quot;worker success!&quot;);  }};</code></pre></field>
        this.exit = new emitter();
        /// <field name='id'><p>Each new worker is given its own unique id, this id is stored in the<code>id</code>.</p><p>While a worker is alive, this is the key that indexes it incluster.workers</p></field>
        this.id = undefined;
        /// <field name='process'><p>All workers are created using <code>child_process.fork()</code>, the returned objectfrom this function is stored in process.</p><p>See: <a href="child_process.html">Child Process module</a></p></field>
        this.process = undefined;
        /// <field name='suicide'><p>This property is a boolean. It is set when a worker dies after calling <code>.destroy()</code>or immediately after calling the <code>.disconnect()</code> method. Until then it is <code>undefined</code>.</p></field>
        this.suicide = undefined;
    }
    /// <field name='settings'><p>All settings set by the <code>.setupMaster</code> is stored in this settings object.This object is not supposed to be change or set manually, by you.</p></field>
    this.settings = undefined;
    /// <field name='isMaster'><p>True if the process is a master. This is determinedby the <code>process.env.NODE_UNIQUE_ID</code>. If <code>process.env.NODE_UNIQUE_ID</code> isundefined, then <code>isMaster</code> is <code>true</code>.</p></field>
    this.isMaster = undefined;
    /// <field name='isWorker'><p>This boolean flag is true if the process is a worker forked from a master.If the <code>process.env.NODE_UNIQUE_ID</code> is set to a value, then<code>isWorker</code> is <code>true</code>.</p></field>
    this.isWorker = undefined;
    /// <field name='worker'><p>A reference to the current worker object. Not available in the master process.</p><pre><code>var cluster = require(&#39;cluster&#39;);if (cluster.isMaster) {  console.log(&#39;I am master&#39;);  cluster.fork();  cluster.fork();} else if (cluster.isWorker) {  console.log(&#39;I am worker #&#39; + cluster.worker.id);}</code></pre></field>
    this.worker = undefined;
    /// <field name='workers'><p>A hash that stores the active worker objects, keyed by <code>id</code> field. Makes iteasy to loop through all the workers. It is only available in the masterprocess.</p><pre><code>// Go through all workersfunction eachWorker(callback) {  for (var id in cluster.workers) {    callback(cluster.workers[id]);  }}eachWorker(function(worker) {  worker.send(&#39;big announcement to all workers&#39;);});</code></pre><p>Should you wish to reference a worker over a communication channel, usingthe worker&#39;s unique id is the easiest way to find the worker.</p><pre><code>socket.on(&#39;data&#39;, function(id) {  var worker = cluster.workers[id];});</code></pre></field>
    this.workers = undefined;
} // module cluster

// **NTVS** INSERT USER MODULES HERE **NTVS** 