//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

// This intellisense header interacts w/ the JavaScript extension points for intellisense
// to reduce the amount of completions that aren't available in Node.js

// First attempt to delete all of the extra variables...
delete ActiveXObject
delete AnimationEvent
delete ApplicationCache
delete Attr
delete Audio
delete AudioTrack
delete AudioTrackList
delete BeforeUnloadEvent
delete Blob
delete BookmarkCollection
delete CDATASection
delete CSSFontFaceRule
delete CSSImportRule
delete CSSKeyframeRule
delete CSSKeyframesRule
delete CSSMediaRule
delete CSSNamespaceRule
delete CSSPageRule
delete CSSRule
delete CSSRuleList
delete CSSStyleDeclaration
delete CSSStyleRule
delete CSSStyleSheet
delete CanvasGradient
delete CanvasPattern
delete CanvasPixelArray
delete CanvasRenderingContext2D
delete CharacterData
delete ClientRect
delete ClientRectList
delete CloseEvent
delete CollectGarbage
delete Comment
delete CompositionEvent
delete Console
delete ControlRangeCollection
delete Coordinates
delete CustomEvent
delete DOMError
delete DOMException
delete DOMImplementation
delete DOMParser
delete DOMSettableTokenList
delete DOMStringList
delete DOMTokenList
delete DataTransfer
delete Debug
delete Document
delete DocumentFragment
delete DocumentType
delete DragEvent
delete Element
delete Enumerator
delete ErrorEvent
delete Event
delete EventException
delete File
delete FileList
delete FileReader
delete FocusEvent
delete FormData
delete Geolocation
delete HTMLAnchorElement
delete HTMLAppletElement
delete HTMLAreaElement
delete HTMLAreasCollection
delete HTMLAudioElement
delete HTMLBGSoundElement
delete HTMLBRElement
delete HTMLBaseElement
delete HTMLBaseFontElement
delete HTMLBlockElement
delete HTMLBodyElement
delete HTMLButtonElement
delete HTMLCanvasElement
delete HTMLCollection
delete HTMLDDElement
delete HTMLDListElement
delete HTMLDTElement
delete HTMLDataListElement
delete HTMLDirectoryElement
delete HTMLDivElement
delete HTMLElement
delete HTMLEmbedElement
delete HTMLFieldSetElement
delete HTMLFontElement
delete HTMLFormElement
delete HTMLFrameElement
delete HTMLFrameSetElement
delete HTMLHRElement
delete HTMLHeadElement
delete HTMLHeadingElement
delete HTMLHtmlElement
delete HTMLIFrameElement
delete HTMLImageElement
delete HTMLInputElement
delete HTMLIsIndexElement
delete HTMLLIElement
delete HTMLLabelElement
delete HTMLLegendElement
delete HTMLLinkElement
delete HTMLMapElement
delete HTMLMarqueeElement
delete HTMLMediaElement
delete HTMLMenuElement
delete HTMLMetaElement
delete HTMLModElement
delete HTMLNextIdElement
delete HTMLOListElement
delete HTMLObjectElement
delete HTMLOptGroupElement
delete HTMLOptionElement
delete HTMLParagraphElement
delete HTMLParamElement
delete HTMLPhraseElement
delete HTMLPreElement
delete HTMLProgressElement
delete HTMLQuoteElement
delete HTMLScriptElement
delete HTMLSelectElement
delete HTMLSourceElement
delete HTMLSpanElement
delete HTMLStyleElement
delete HTMLTableCaptionElement
delete HTMLTableCellElement
delete HTMLTableColElement
delete HTMLTableDataCellElement
delete HTMLTableElement
delete HTMLTableHeaderCellElement
delete HTMLTableRowElement
delete HTMLTableSectionElement
delete HTMLTextAreaElement
delete HTMLTitleElement
delete HTMLTrackElement
delete HTMLUListElement
delete HTMLUnknownElement
delete HTMLVideoElement
delete History
delete IDBCursor
delete IDBCursorWithValue
delete IDBDatabase
delete IDBFactory
delete IDBIndex
delete IDBKeyRange
delete IDBObjectStore
delete IDBOpenDBRequest
delete IDBRequest
delete IDBTransaction
delete IDBVersionChangeEvent
delete Image
delete ImageData
delete KeyboardEvent
delete Location
delete MSBehaviorUrnsCollection
delete MSBlobBuilder
delete MSCSSMatrix
delete MSCSSProperties
delete MSCSSRuleList
delete MSCompatibleInfo
delete MSCompatibleInfoCollection
delete MSCurrentStyleCSSProperties
delete MSEventObj
delete MSGesture
delete MSGestureEvent
delete MSManipulationEvent
delete MSMimeTypesCollection
delete MSPluginsCollection
delete MSPointerEvent
delete MSPopupWindow
delete MSRangeCollection
delete MSSelection
delete MSSiteModeEvent
delete MSStream
delete MSStreamReader
delete MSStyleCSSProperties
delete MediaError
delete MediaList
delete MediaQueryList
delete MessageChannel
delete MessageEvent
delete MessagePort
delete MouseEvent
delete MouseWheelEvent
delete MutationEvent
delete NamedNodeMap
delete Navigator
delete Node
delete NodeFilter
delete NodeIterator
delete NodeList
delete Option
delete Performance
delete PerformanceEntry
delete PerformanceMark
delete PerformanceMeasure
delete PerformanceNavigation
delete PerformanceResourceTiming
delete PerformanceTiming
delete PopStateEvent
delete Position
delete PositionError
delete ProcessingInstruction
delete ProgressEvent
delete Range
delete RangeException
delete SVGAElement
delete SVGAngle
delete SVGAnimatedAngle
delete SVGAnimatedBoolean
delete SVGAnimatedEnumeration
delete SVGAnimatedInteger
delete SVGAnimatedLength
delete SVGAnimatedLengthList
delete SVGAnimatedNumber
delete SVGAnimatedNumberList
delete SVGAnimatedPreserveAspectRatio
delete SVGAnimatedRect
delete SVGAnimatedString
delete SVGAnimatedTransformList
delete SVGCircleElement
delete SVGClipPathElement
delete SVGComponentTransferFunctionElement
delete SVGDefsElement
delete SVGDescElement
delete SVGElement
delete SVGElementInstance
delete SVGElementInstanceList
delete SVGEllipseElement
delete SVGException
delete SVGFEBlendElement
delete SVGFEColorMatrixElement
delete SVGFEComponentTransferElement
delete SVGFECompositeElement
delete SVGFEConvolveMatrixElement
delete SVGFEDiffuseLightingElement
delete SVGFEDisplacementMapElement
delete SVGFEDistantLightElement
delete SVGFEFloodElement
delete SVGFEFuncAElement
delete SVGFEFuncBElement
delete SVGFEFuncGElement
delete SVGFEFuncRElement
delete SVGFEGaussianBlurElement
delete SVGFEImageElement
delete SVGFEMergeElement
delete SVGFEMergeNodeElement
delete SVGFEMorphologyElement
delete SVGFEOffsetElement
delete SVGFEPointLightElement
delete SVGFESpecularLightingElement
delete SVGFESpotLightElement
delete SVGFETileElement
delete SVGFETurbulenceElement
delete SVGFilterElement
delete SVGGElement
delete SVGGradientElement
delete SVGImageElement
delete SVGLength
delete SVGLengthList
delete SVGLineElement
delete SVGLinearGradientElement
delete SVGMarkerElement
delete SVGMaskElement
delete SVGMatrix
delete SVGMetadataElement
delete SVGNumber
delete SVGNumberList
delete SVGPathElement
delete SVGPathSeg
delete SVGPathSegArcAbs
delete SVGPathSegArcRel
delete SVGPathSegClosePath
delete SVGPathSegCurvetoCubicAbs
delete SVGPathSegCurvetoCubicRel
delete SVGPathSegCurvetoCubicSmoothAbs
delete SVGPathSegCurvetoCubicSmoothRel
delete SVGPathSegCurvetoQuadraticAbs
delete SVGPathSegCurvetoQuadraticRel
delete SVGPathSegCurvetoQuadraticSmoothAbs
delete SVGPathSegCurvetoQuadraticSmoothRel
delete SVGPathSegLinetoAbs
delete SVGPathSegLinetoHorizontalAbs
delete SVGPathSegLinetoHorizontalRel
delete SVGPathSegLinetoRel
delete SVGPathSegLinetoVerticalAbs
delete SVGPathSegLinetoVerticalRel
delete SVGPathSegList
delete SVGPathSegMovetoAbs
delete SVGPathSegMovetoRel
delete SVGPatternElement
delete SVGPoint
delete SVGPointList
delete SVGPolygonElement
delete SVGPolylineElement
delete SVGPreserveAspectRatio
delete SVGRadialGradientElement
delete SVGRect
delete SVGRectElement
delete SVGSVGElement
delete SVGScriptElement
delete SVGStopElement
delete SVGStringList
delete SVGStyleElement
delete SVGSwitchElement
delete SVGSymbolElement
delete SVGTSpanElement
delete SVGTextContentElement
delete SVGTextElement
delete SVGTextPathElement
delete SVGTextPositioningElement
delete SVGTitleElement
delete SVGTransform
delete SVGTransformList
delete SVGUnitTypes
delete SVGUseElement
delete SVGViewElement
delete SVGZoomAndPan
delete SVGZoomEvent
delete Screen
delete ScriptEngine
delete ScriptEngineBuildVersion
delete ScriptEngineMajorVersion
delete ScriptEngineMinorVersion
delete Selection
delete Storage
delete StorageEvent
delete StyleMedia
delete StyleSheet
delete StyleSheetList
delete StyleSheetPageList
delete Text
delete TextEvent
delete TextMetrics
delete TextRange
delete TextRangeCollection
delete TextTrack
delete TextTrackCue
delete TextTrackCueList
delete TextTrackList
delete TimeRanges
delete TrackEvent
delete TransitionEvent
delete TreeWalker
delete UIEvent
delete URL
delete VBArray
delete ValidityState
delete WebSocket
delete WheelEvent
delete Window
delete Worker
delete XDomainRequest
delete XMLHttpRequest
delete XMLHttpRequestEventTarget
delete XMLSerializer
delete addEventListener
delete alert
delete animationStartTime
delete applicationCache
delete atob
delete attachEvent
delete blur
delete btoa
delete cancelAnimationFrame
delete clientInformation
delete clipboardData
delete close
delete closed
delete confirm
delete createPopup
delete defaultStatus
delete detachEvent
delete dispatchEvent
delete document
delete emitter
delete event
delete execScript
delete external
delete focus
delete frameElement
delete frames
delete getComputedStyle
delete getSelection
delete history
delete indexedDB
delete innerHeight
delete innerWidth
delete item
delete length
delete localStorage
delete location
delete matchMedia
delete maxConnectionsPerServer
delete module_body
delete moveBy
delete moveTo
delete msAnimationStartTime
delete msCancelRequestAnimationFrame
delete msClearImmediate
delete msIndexedDB
delete msIsStaticHTML
delete msMatchMedia
delete msRequestAnimationFrame
delete msSetImmediate
delete msWriteProfilerMark
delete name
delete navigate
delete navigator
delete offscreenBuffering
delete onerror
delete open
delete opener
delete outerHeight
delete outerWidth
delete pageXOffset
delete pageYOffset
delete parent
delete performance
delete postMessage
delete print
delete prompt
delete removeEventListener
delete requestAnimationFrame
delete resizeBy
delete resizeTo
delete screen
delete screenLeft
delete screenTop
delete screenX
delete screenY
delete scroll
delete scrollBy
delete scrollTo
delete self
delete sessionStorage
delete showHelp
delete showModalDialog
delete showModelessDialog
delete status
delete toStaticHTML
delete top
delete window

// Then setup a filter for what we can't delete...
var nodejs_tools_for_visual_studio_hidden_names = {"VBArray":null,  "Enumerator":null,  "document":null,  "window":null,  "Worker":null,  "MSCSSMatrix":null,  "WebSocket"};
var nodejs_tools_for_visual_studio_hidden_properties = {"onmspointerdown":null,  "onmsgesturedoubletap":null,  "onmouseleave":null,  "onmspointerhover":null,  "onmouseenter":null,  "onmspointermove":null,  "onmsgesturehold":null,  "onmsgesturechange":null,  "onmsgesturestart":null,  "onhelp":null,  "onmspointercancel":null,  "onmsgesturetap":null,  "onmsgestureend":null,  "onmspointerout":null,  "onfocusout":null,  "onmsinertiastart":null,  "onfocusin":null,  "onmspointerover":null,  "onmspointerup":null,  "ondragend":null,  "onkeydown":null,  "ondragover":null,  "onkeyup":null,  "onreset":null,  "onmouseup":null,  "ondragstart":null,  "ondrag":null,  "onmouseover":null,  "ondragleave":null,  "onafterprint":null,  "onpause":null,  "onbeforeprint":null,  "onseeked":null,  "onmousedown":null,  "onclick":null,  "onwaiting":null,  "ononline":null,  "ondurationchange":null,  "onblur":null,  "onemptied":null,  "onseeking":null,  "oncanplay":null,  "onstalled":null,  "onmousemove":null,  "onoffline":null,  "onbeforeunload":null,  "onpopstate":null,  "onstorage":null,  "onratechange":null,  "onloadstart":null,  "ondragenter":null,  "onsubmit":null,  "onprogress":null,  "ondblclick":null,  "oncontextmenu":null,  "onchange":null,  "onloadedmetadata":null,  "onplay":null,  "onplaying":null,  "oncanplaythrough":null,  "onabort":null,  "onreadystatechange":null,  "onkeypress":null,  "onloadeddata":null,  "onsuspend":null,  "onfocus":null,  "onmessage":null,  "ontimeupdate":null,  "onresize":null,  "onselect":null,  "ondrop":null,  "onmouseout":null,  "onended":null,  "onunload":null,  "onhashchange":null,  "onscroll":null,  "onmousewheel":null,  "onvolumechange":null,  "onload":null,  "oninput"};
var nodejs_tools_for_visual_studio_hidden_objects = [VBArray, Enumerator, document];

intellisense.addEventListener('statementcompletion', function (event) {
    if (event.scope == 'global') {
        event.items = event.items.filter(function (item) {
            if (item.name.indexOf("nodejs_tools_for_visual_studio_hidden_") == 0) {
                // hide our internal variables from intellisense
                return false;
            }
            if (item.kind == "property" && item.name in nodejs_tools_for_visual_studio_hidden_properties) {
                // hide all of the known properties
                return false;
            }
            if (item.kind == "field" || item.kind == "property") {
                if (typeof item.value == 'undefined' && item.name in nodejs_tools_for_visual_studio_hidden_names) {
                    // hide various fields and properties that we can't delete
                    // We check item.value to be undefined because for user defined values
                    // we'll have a value.
                    return false;
                }
            }
            if (item.kind == "method" || item.kind == "field") {
                if (nodejs_tools_for_visual_studio_hidden_objects.indexOf(item.value) != -1) {
                    // hide VBArray & Enumerator which we can't delete and still have values.
                    return false;
                }
            }
            return true;
        });
    }
});

global = {};
require = function () {
    var require_count = 0;
    var require_depth = 0;
    var max_require_depth = 5;
    var cache = {
        "timers": null,
        "module": null,
        "addons": null,
        "util": null,
        "events": null,
        "domain": null,
        "buffer": null,
        "stream": null,
        "crypto": null,
        "tls": null,
        "stringdecoder": null,
        "fs": null,
        "path": null,
        "net": null,
        "dgram": null,
        "dns": null,
        "http": null,
        "https": null,
        "url": null,
        "querystring": null,
        "punycode": null,
        "readline": null,
        "repl": null,
        "vm": null,
        "child_process": null,
        "assert": null,
        "tty": null,
        "zlib": null,
        "os": null,
        "cluster": null,
    }
    function make_module(module_name) {
        switch(module_name) { 
            case "timers": return new     function timers() {
                /// <summary><p>All of the timer functions are globals.  You do not need to <code>require()</code>&#10;this module in order to use them.&#10;&#10;</p>&#10;</summary>
                this.setTimeout = function(callback, delay, arg) {
                    /// <summary><p>To schedule execution of a one-time <code>callback</code> after <code>delay</code> milliseconds. Returns a&#10;<code>timeoutId</code> for possible use with <code>clearTimeout()</code>. Optionally you can&#10;also pass arguments to the callback.&#10;&#10;</p>&#10;<p>It is important to note that your callback will probably not be called in exactly&#10;<code>delay</code> milliseconds - Node.js makes no guarantees about the exact timing of when&#10;the callback will fire, nor of the ordering things will fire in. The callback will&#10;be called as close as possible to the time specified.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="callback"></param>
                    /// <param name="delay"></param>
                    /// <param name="arg"></param>
                    /// <param name="..."></param>
                    /// </signature>
                }
                this.clearTimeout = function(timeoutId) {
                    /// <summary><p>Prevents a timeout from triggering.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="timeoutId"></param>
                    /// </signature>
                }
                this.setInterval = function(callback, delay, arg) {
                    /// <summary><p>To schedule the repeated execution of <code>callback</code> every <code>delay</code> milliseconds.&#10;Returns a <code>intervalId</code> for possible use with <code>clearInterval()</code>. Optionally&#10;you can also pass arguments to the callback.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="callback"></param>
                    /// <param name="delay"></param>
                    /// <param name="arg"></param>
                    /// <param name="..."></param>
                    /// </signature>
                }
                this.clearInterval = function(intervalId) {
                    /// <summary><p>Stops a interval from triggering.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="intervalId"></param>
                    /// </signature>
                }
                this.unref = function() {
                    /// <summary><p>The opaque value returned by <code>setTimeout</code> and <code>setInterval</code> also has the method&#10;<code>timer.unref()</code> which will allow you to create a timer that is active but if&#10;it is the only item left in the event loop won&#39;t keep the program running.&#10;If the timer is already <code>unref</code>d calling <code>unref</code> again will have no effect.&#10;&#10;</p>&#10;<p>In the case of <code>setTimeout</code> when you <code>unref</code> you create a separate timer that&#10;will wakeup the event loop, creating too many of these may adversely effect&#10;event loop performance -- use wisely.&#10;&#10;</p>&#10;</summary>
                }
                this.ref = function() {
                    /// <summary><p>If you had previously <code>unref()</code>d a timer you can call <code>ref()</code> to explicitly&#10;request the timer hold the program open. If the timer is already <code>ref</code>d calling&#10;<code>ref</code> again will have no effect.&#10;&#10;</p>&#10;</summary>
                }
                this.setImmediate = function(callback, arg) {
                    /// <summary><p>To schedule the &quot;immediate&quot; execution of <code>callback</code> after I/O events&#10;callbacks and before <code>setTimeout</code> and <code>setInterval</code> . Returns an&#10;<code>immediateId</code> for possible use with <code>clearImmediate()</code>. Optionally you&#10;can also pass arguments to the callback.&#10;&#10;</p>&#10;<p>Immediates are queued in the order created, and are popped off the queue once&#10;per loop iteration. This is different from <code>process.nextTick</code> which will&#10;execute <code>process.maxTickDepth</code> queued callbacks per iteration. <code>setImmediate</code>&#10;will yield to the event loop after firing a queued callback to make sure I/O is&#10;not being starved. While order is preserved for execution, other I/O events may&#10;fire between any two scheduled immediate callbacks.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="callback"></param>
                    /// <param name="arg"></param>
                    /// <param name="..."></param>
                    /// </signature>
                }
                this.clearImmediate = function(immediateId) {
                    /// <summary><p>Stops an immediate from triggering.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="immediateId"></param>
                    /// </signature>
                }
            };
            case "module": return new     function module() {
                /// <summary><p>Node has a simple module loading system.  In Node, files and modules are in&#10;one-to-one correspondence.  As an example, <code>foo.js</code> loads the module&#10;<code>circle.js</code> in the same directory.&#10;&#10;</p>&#10;<p>The contents of <code>foo.js</code>:&#10;&#10;</p>&#10;<pre><code>var circle = require(&#39;./circle.js&#39;);&#10;console.log( &#39;The area of a circle of radius 4 is &#39;&#10;           + circle.area(4));</code></pre>&#10;<p>The contents of <code>circle.js</code>:&#10;&#10;</p>&#10;<pre><code>var PI = Math.PI;&#10;&#10;exports.area = function (r) {&#10;  return PI * r * r;&#10;};&#10;&#10;exports.circumference = function (r) {&#10;  return 2 * PI * r;&#10;};</code></pre>&#10;<p>The module <code>circle.js</code> has exported the functions <code>area()</code> and&#10;<code>circumference()</code>.  To export an object, add to the special <code>exports</code>&#10;object.&#10;&#10;</p>&#10;<p>Note that <code>exports</code> is a reference to <code>module.exports</code> making it suitable&#10;for augmentation only. If you are exporting a single item such as a&#10;constructor you will want to use <code>module.exports</code> directly instead.&#10;&#10;</p>&#10;<pre><code>function MyConstructor (opts) {&#10;  //...&#10;}&#10;&#10;// BROKEN: Does not modify exports&#10;exports = MyConstructor;&#10;&#10;// exports the constructor properly&#10;module.exports = MyConstructor;</code></pre>&#10;<p>Variables&#10;local to the module will be private. In this example the variable <code>PI</code> is&#10;private to <code>circle.js</code>.&#10;&#10;</p>&#10;<p>The module system is implemented in the <code>require(&quot;module&quot;)</code> module.&#10;&#10;</p>&#10;</summary>
            };
            case "addons": return new     function addons() {
                /// <summary><p>Addons are dynamically linked shared objects. They can provide glue to C and&#10;C++ libraries. The API (at the moment) is rather complex, involving&#10;knowledge of several libraries:&#10;&#10;</p>&#10;<ul>&#10;<li><p>V8 JavaScript, a C++ library. Used for interfacing with JavaScript:&#10;creating objects, calling functions, etc.  Documented mostly in the&#10;<code>v8.h</code> header file (<code>deps/v8/include/v8.h</code> in the Node source&#10;tree), which is also available&#10;<a href="http://izs.me/v8-docs/main.html">online</a>.</p>&#10;</li>&#10;<li><p><a href="https://github.com/joyent/libuv">libuv</a>, C event loop library.&#10;Anytime one needs to wait for a file descriptor to become readable,&#10;wait for a timer, or wait for a signal to be received one will need&#10;to interface with libuv. That is, if you perform any I/O, libuv will&#10;need to be used.</p>&#10;</li>&#10;<li><p>Internal Node libraries. Most importantly is the <code>node::ObjectWrap</code>&#10;class which you will likely want to derive from.</p>&#10;</li>&#10;<li><p>Others. Look in <code>deps/</code> for what else is available.</p>&#10;</li>&#10;</ul>&#10;<p>Node statically compiles all its dependencies into the executable.&#10;When compiling your module, you don&#39;t need to worry about linking to&#10;any of these libraries.&#10;&#10;</p>&#10;<p>All of the following examples are available for&#10;<a href="https://github.com/rvagg/node-addon-examples">download</a> and may be&#10;used as a starting-point for your own Addon.&#10;&#10;</p>&#10;</summary>
            };
            case "util": return new     function util() {
                /// <summary><p>These functions are in the module <code>&#39;util&#39;</code>. Use <code>require(&#39;util&#39;)</code> to access&#10;them.&#10;&#10;&#10;</p>&#10;</summary>
                this.format = function(format) {
                    /// <summary><p>Returns a formatted string using the first argument as a <code>printf</code>-like format.&#10;&#10;</p>&#10;<p>The first argument is a string that contains zero or more <em>placeholders</em>.&#10;Each placeholder is replaced with the converted value from its corresponding&#10;argument. Supported placeholders are:&#10;&#10;</p>&#10;<ul>&#10;<li><code>%s</code> - String.</li>&#10;<li><code>%d</code> - Number (both integer and float).</li>&#10;<li><code>%j</code> - JSON.</li>&#10;<li><code>%</code> - single percent sign (<code>&#39;%&#39;</code>). This does not consume an argument.</li>&#10;</ul>&#10;<p>If the placeholder does not have a corresponding argument, the placeholder is&#10;not replaced.&#10;&#10;</p>&#10;<pre><code>util.format(&#39;%s:%s&#39;, &#39;foo&#39;); // &#39;foo:%s&#39;</code></pre>&#10;<p>If there are more arguments than placeholders, the extra arguments are&#10;converted to strings with <code>util.inspect()</code> and these strings are concatenated,&#10;delimited by a space.&#10;&#10;</p>&#10;<pre><code>util.format(&#39;%s:%s&#39;, &#39;foo&#39;, &#39;bar&#39;, &#39;baz&#39;); // &#39;foo:bar baz&#39;</code></pre>&#10;<p>If the first argument is not a format string then <code>util.format()</code> returns&#10;a string that is the concatenation of all its arguments separated by spaces.&#10;Each argument is converted to a string with <code>util.inspect()</code>.&#10;&#10;</p>&#10;<pre><code>util.format(1, 2, 3); // &#39;1 2 3&#39;</code></pre>&#10;</summary>
                    /// <signature>
                    /// <param name="format"></param>
                    /// <param name="..."></param>
                    /// </signature>
                }
                this.debug = function(string) {
                    /// <summary><p>A synchronous output function. Will block the process and&#10;output <code>string</code> immediately to <code>stderr</code>.&#10;&#10;</p>&#10;<pre><code>require(&#39;util&#39;).debug(&#39;message on stderr&#39;);</code></pre>&#10;</summary>
                    /// <signature>
                    /// <param name="string"></param>
                    /// </signature>
                }
                this.error = function() {
                    /// <summary><p>Same as <code>util.debug()</code> except this will output all arguments immediately to&#10;<code>stderr</code>.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="..."></param>
                    /// </signature>
                }
                this.puts = function() {
                    /// <summary><p>A synchronous output function. Will block the process and output all arguments&#10;to <code>stdout</code> with newlines after each argument.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="..."></param>
                    /// </signature>
                }
                this.print = function() {
                    /// <summary><p>A synchronous output function. Will block the process, cast each argument to a&#10;string then output to <code>stdout</code>. Does not place newlines after each argument.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="..."></param>
                    /// </signature>
                }
                this.log = function(string) {
                    /// <summary><p>Output with timestamp on <code>stdout</code>.&#10;&#10;</p>&#10;<pre><code>require(&#39;util&#39;).log(&#39;Timestamped message.&#39;);</code></pre>&#10;</summary>
                    /// <signature>
                    /// <param name="string"></param>
                    /// </signature>
                }
                this.inspect = function(object, options) {
                    /// <summary><p>Return a string representation of <code>object</code>, which is useful for debugging.&#10;&#10;</p>&#10;<p>An optional <em>options</em> object may be passed that alters certain aspects of the&#10;formatted string:&#10;&#10;</p>&#10;<ul>&#10;<li><p><code>showHidden</code> - if <code>true</code> then the object&#39;s non-enumerable properties will be&#10;shown too. Defaults to <code>false</code>.</p>&#10;</li>&#10;<li><p><code>depth</code> - tells <code>inspect</code> how many times to recurse while formatting the&#10;object. This is useful for inspecting large complicated objects. Defaults to&#10;<code>2</code>. To make it recurse indefinitely pass <code>null</code>.</p>&#10;</li>&#10;<li><p><code>colors</code> - if <code>true</code>, then the output will be styled with ANSI color codes.&#10;Defaults to <code>false</code>. Colors are customizable, see below.</p>&#10;</li>&#10;<li><p><code>customInspect</code> - if <code>false</code>, then custom <code>inspect()</code> functions defined on the&#10;objects being inspected won&#39;t be called. Defaults to <code>true</code>.</p>&#10;</li>&#10;</ul>&#10;<p>Example of inspecting all properties of the <code>util</code> object:&#10;&#10;</p>&#10;<pre><code>var util = require(&#39;util&#39;);&#10;&#10;console.log(util.inspect(util, { showHidden: true, depth: null }));</code></pre>&#10;</summary>
                    /// <signature>
                    /// <param name="object"></param>
                    /// <param name="options"></param>
                    /// </signature>
                }
                this.isArray = function(object) {
                    /// <summary><p>Returns <code>true</code> if the given &quot;object&quot; is an <code>Array</code>. <code>false</code> otherwise.&#10;&#10;</p>&#10;<pre><code>var util = require(&#39;util&#39;);&#10;&#10;util.isArray([])&#10;  // true&#10;util.isArray(new Array)&#10;  // true&#10;util.isArray({})&#10;  // false</code></pre>&#10;</summary>
                    /// <signature>
                    /// <param name="object"></param>
                    /// </signature>
                }
                this.isRegExp = function(object) {
                    /// <summary><p>Returns <code>true</code> if the given &quot;object&quot; is a <code>RegExp</code>. <code>false</code> otherwise.&#10;&#10;</p>&#10;<pre><code>var util = require(&#39;util&#39;);&#10;&#10;util.isRegExp(/some regexp/)&#10;  // true&#10;util.isRegExp(new RegExp(&#39;another regexp&#39;))&#10;  // true&#10;util.isRegExp({})&#10;  // false</code></pre>&#10;</summary>
                    /// <signature>
                    /// <param name="object"></param>
                    /// </signature>
                }
                this.isDate = function(object) {
                    /// <summary><p>Returns <code>true</code> if the given &quot;object&quot; is a <code>Date</code>. <code>false</code> otherwise.&#10;&#10;</p>&#10;<pre><code>var util = require(&#39;util&#39;);&#10;&#10;util.isDate(new Date())&#10;  // true&#10;util.isDate(Date())&#10;  // false (without &#39;new&#39; returns a String)&#10;util.isDate({})&#10;  // false</code></pre>&#10;</summary>
                    /// <signature>
                    /// <param name="object"></param>
                    /// </signature>
                }
                this.isError = function(object) {
                    /// <summary><p>Returns <code>true</code> if the given &quot;object&quot; is an <code>Error</code>. <code>false</code> otherwise.&#10;&#10;</p>&#10;<pre><code>var util = require(&#39;util&#39;);&#10;&#10;util.isError(new Error())&#10;  // true&#10;util.isError(new TypeError())&#10;  // true&#10;util.isError({ name: &#39;Error&#39;, message: &#39;an error occurred&#39; })&#10;  // false</code></pre>&#10;</summary>
                    /// <signature>
                    /// <param name="object"></param>
                    /// </signature>
                }
                this.pump = function(readableStream, writableStream, callback) {
                    /// <summary><p>Read the data from <code>readableStream</code> and send it to the <code>writableStream</code>.&#10;When <code>writableStream.write(data)</code> returns <code>false</code> <code>readableStream</code> will be&#10;paused until the <code>drain</code> event occurs on the <code>writableStream</code>. <code>callback</code> gets&#10;an error as its only argument and is called when <code>writableStream</code> is closed or&#10;when an error occurs.&#10;&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="readableStream"></param>
                    /// <param name="writableStream"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.inherits = function(constructor, superConstructor) {
                    /// <summary><p>Inherit the prototype methods from one&#10;<a href="https://developer.mozilla.org/en/JavaScript/Reference/Global_Objects/Object/constructor">constructor</a>&#10;into another.  The prototype of <code>constructor</code> will be set to a new&#10;object created from <code>superConstructor</code>.&#10;&#10;</p>&#10;<p>As an additional convenience, <code>superConstructor</code> will be accessible&#10;through the <code>constructor.super_</code> property.&#10;&#10;</p>&#10;<pre><code>var util = require(&quot;util&quot;);&#10;var events = require(&quot;events&quot;);&#10;&#10;function MyStream() {&#10;    events.EventEmitter.call(this);&#10;}&#10;&#10;util.inherits(MyStream, events.EventEmitter);&#10;&#10;MyStream.prototype.write = function(data) {&#10;    this.emit(&quot;data&quot;, data);&#10;}&#10;&#10;var stream = new MyStream();&#10;&#10;console.log(stream instanceof events.EventEmitter); // true&#10;console.log(MyStream.super_ === events.EventEmitter); // true&#10;&#10;stream.on(&quot;data&quot;, function(data) {&#10;    console.log(&#39;Received data: &quot;&#39; + data + &#39;&quot;&#39;);&#10;})&#10;stream.write(&quot;It works!&quot;); // Received data: &quot;It works!&quot;</code></pre>&#10;</summary>
                    /// <signature>
                    /// <param name="constructor"></param>
                    /// <param name="superConstructor"></param>
                    /// </signature>
                }
            };
            case "events": return new     function events() {
                /// <summary><p>Many objects in Node emit events: a <code>net.Server</code> emits an event each time&#10;a peer connects to it, a <code>fs.readStream</code> emits an event when the file is&#10;opened. All objects which emit events are instances of <code>events.EventEmitter</code>.&#10;You can access this module by doing: <code>require(&quot;events&quot;);</code>&#10;&#10;</p>&#10;<p>Typically, event names are represented by a camel-cased string, however,&#10;there aren&#39;t any strict restrictions on that, as any string will be accepted.&#10;&#10;</p>&#10;<p>Functions can then be attached to objects, to be executed when an event&#10;is emitted. These functions are called <em>listeners</em>. Inside a listener&#10;function, <code>this</code> refers to the <code>EventEmitter</code> that the listener was&#10;attached to.&#10;&#10;&#10;</p>&#10;</summary>
                function _EventEmitter() {
                    this.addListener = function(event, listener) {
                        /// <summary><p>Adds a listener to the end of the listeners array for the specified event.&#10;&#10;</p>&#10;<pre><code>server.on(&#39;connection&#39;, function (stream) {&#10;  console.log(&#39;someone connected!&#39;);&#10;});</code></pre>&#10;<p>Returns emitter, so calls can be chained.&#10;&#10;</p>&#10;</summary>
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
                        /// <summary><p>Adds a listener to the end of the listeners array for the specified event.&#10;&#10;</p>&#10;<pre><code>server.on(&#39;connection&#39;, function (stream) {&#10;  console.log(&#39;someone connected!&#39;);&#10;});</code></pre>&#10;<p>Returns emitter, so calls can be chained.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="event"></param>
                        /// <param name="listener"></param>
                        /// </signature>
                    }
                    this.once = function(event, listener) {
                        /// <summary><p>Adds a <strong>one time</strong> listener for the event. This listener is&#10;invoked only the next time the event is fired, after which&#10;it is removed.&#10;&#10;</p>&#10;<pre><code>server.once(&#39;connection&#39;, function (stream) {&#10;  console.log(&#39;Ah, we have our first user!&#39;);&#10;});</code></pre>&#10;<p>Returns emitter, so calls can be chained.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="event"></param>
                        /// <param name="listener"></param>
                        /// </signature>
                    }
                    this.removeListener = function(event, listener) {
                        /// <summary><p>Remove a listener from the listener array for the specified event.&#10;<strong>Caution</strong>: changes array indices in the listener array behind the listener.&#10;&#10;</p>&#10;<pre><code>var callback = function(stream) {&#10;  console.log(&#39;someone connected!&#39;);&#10;};&#10;server.on(&#39;connection&#39;, callback);&#10;// ...&#10;server.removeListener(&#39;connection&#39;, callback);</code></pre>&#10;<p>Returns emitter, so calls can be chained.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="event"></param>
                        /// <param name="listener"></param>
                        /// </signature>
                    }
                    this.removeAllListeners = function(event) {
                        /// <summary><p>Removes all listeners, or those of the specified event.&#10;&#10;</p>&#10;<p>Returns emitter, so calls can be chained.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="event"></param>
                        /// </signature>
                    }
                    this.setMaxListeners = function(n) {
                        /// <summary><p>By default EventEmitters will print a warning if more than 10 listeners are&#10;added for a particular event. This is a useful default which helps finding memory leaks.&#10;Obviously not all Emitters should be limited to 10. This function allows&#10;that to be increased. Set to zero for unlimited.&#10;&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="n"></param>
                        /// </signature>
                    }
                    this.listeners = function(event) {
                        /// <summary><p>Returns an array of listeners for the specified event.&#10;&#10;</p>&#10;<pre><code>server.on(&#39;connection&#39;, function (stream) {&#10;  console.log(&#39;someone connected!&#39;);&#10;});&#10;console.log(util.inspect(server.listeners(&#39;connection&#39;))); // [ [Function] ]</code></pre>&#10;</summary>
                        /// <signature>
                        /// <param name="event"></param>
                        /// </signature>
                    }
                    this.emit = function(event, arg1, arg2) {
                        /// <summary><p>Execute each of the listeners in order with the supplied arguments.&#10;&#10;</p>&#10;<p>Returns <code>true</code> if event had listeners, <code>false</code> otherwise.&#10;&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="event"></param>
                        /// <param name="arg1"></param>
                        /// <param name="arg2"></param>
                        /// <param name="..."></param>
                        /// </signature>
                    }
                    this.addListener = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// newListener: This event is emitted any time someone adds a new listener.  It is unspecified ...&#10;
                        /// removeListener: This event is emitted any time someone removes a listener.  It is unspecified ...&#10;
                        /// </summary>

                    }
                    this.once = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// newListener: This event is emitted any time someone adds a new listener.  It is unspecified ...&#10;
                        /// removeListener: This event is emitted any time someone removes a listener.  It is unspecified ...&#10;
                        /// </summary>

                    }
                    this.removeListener = function(event, listener) {
                        /// <summary>Supported Events: newListener, removeListener</summary>
                    }
                    this.removeAllListeners = function(event) {
                        /// <summary>Supported Events: newListener, removeListener</summary>
                    }
                    this.setMaxListeners = function(n) { }
                    this.listeners = function(event) {
                        /// <summary>Supported Events: newListener, removeListener</summary>
                    }
                    this.emit = function(event, arguments) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// newListener: This event is emitted any time someone adds a new listener.  It is unspecified ...&#10;
                        /// removeListener: This event is emitted any time someone removes a listener.  It is unspecified ...&#10;
                        /// </summary>

                    }
                    this.on = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// newListener: This event is emitted any time someone adds a new listener.  It is unspecified ...&#10;
                        /// removeListener: This event is emitted any time someone removes a listener.  It is unspecified ...&#10;
                        /// </summary>

                    }
                }

                this.EventEmitter = function() {
                    return new _EventEmitter();
                }
            };
            case "domain": return new     function domain() {
                /// <summary><p>Domains provide a way to handle multiple different IO operations as a&#10;single group.  If any of the event emitters or callbacks registered to a&#10;domain emit an <code>error</code> event, or throw an error, then the domain object&#10;will be notified, rather than losing the context of the error in the&#10;<code>process.on(&#39;uncaughtException&#39;)</code> handler, or causing the program to&#10;exit immediately with an error code.&#10;&#10;</p>&#10;</summary>
                this.create = function() {
                    /// <summary><p>Returns a new Domain object.&#10;&#10;</p>&#10;</summary>
                }
                function _Domain() {
                    this.run = function(fn) {
                        /// <summary><p>Run the supplied function in the context of the domain, implicitly&#10;binding all event emitters, timers, and lowlevel requests that are&#10;created in that context.&#10;&#10;</p>&#10;<p>This is the most basic way to use a domain.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>var d = domain.create();&#10;d.on(&#39;error&#39;, function(er) {&#10;  console.error(&#39;Caught error!&#39;, er);&#10;});&#10;d.run(function() {&#10;  process.nextTick(function() {&#10;    setTimeout(function() { // simulating some various async stuff&#10;      fs.open(&#39;non-existent file&#39;, &#39;r&#39;, function(er, fd) {&#10;        if (er) throw er;&#10;        // proceed...&#10;      });&#10;    }, 100);&#10;  });&#10;});</code></pre>&#10;<p>In this example, the <code>d.on(&#39;error&#39;)</code> handler will be triggered, rather&#10;than crashing the program.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="fn" type="Function"></param>
                        /// </signature>
                        /// <signature>
                        /// <param name="fn"></param>
                        /// </signature>
                    }
                    this.add = function(emitter) {
                        /// <summary><p>Explicitly adds an emitter to the domain.  If any event handlers called by&#10;the emitter throw an error, or if the emitter emits an <code>error</code> event, it&#10;will be routed to the domain&#39;s <code>error</code> event, just like with implicit&#10;binding.&#10;&#10;</p>&#10;<p>This also works with timers that are returned from <code>setInterval</code> and&#10;<code>setTimeout</code>.  If their callback function throws, it will be caught by&#10;the domain &#39;error&#39; handler.&#10;&#10;</p>&#10;<p>If the Timer or EventEmitter was already bound to a domain, it is removed&#10;from that one, and bound to this one instead.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="emitter" type="EventEmitter">emitter or timer to be added to the domain</param>
                        /// </signature>
                        /// <signature>
                        /// <param name="emitter"></param>
                        /// </signature>
                    }
                    this.remove = function(emitter) {
                        /// <summary><p>The opposite of <code>domain.add(emitter)</code>.  Removes domain handling from the&#10;specified emitter.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="emitter" type="EventEmitter">emitter or timer to be removed from the domain</param>
                        /// </signature>
                        /// <signature>
                        /// <param name="emitter"></param>
                        /// </signature>
                    }
                    this.bind = function(callback) {
                        /// <summary><p>The returned function will be a wrapper around the supplied callback&#10;function.  When the returned function is called, any errors that are&#10;thrown will be routed to the domain&#39;s <code>error</code> event.&#10;&#10;</p>&#10;<h4>Example</h4>&#10;<pre><code>var d = domain.create();&#10;&#10;function readSomeFile(filename, cb) {&#10;  fs.readFile(filename, &#39;utf8&#39;, d.bind(function(er, data) {&#10;    // if this throws, it will also be passed to the domain&#10;    return cb(er, data ? JSON.parse(data) : null);&#10;  }));&#10;}&#10;&#10;d.on(&#39;error&#39;, function(er) {&#10;  // an error occurred somewhere.&#10;  // if we throw it now, it will crash the program&#10;  // with the normal line number and stack message.&#10;});</code></pre>&#10;</summary>
                        /// <signature>
                        /// <param name="callback" type="Function">The callback function</param>
                        /// <returns type="Function">The bound function</returns>
                        /// </signature>
                        /// <signature>
                        /// <param name="callback"></param>
                        /// </signature>
                    }
                    this.intercept = function(callback) {
                        /// <summary><p>This method is almost identical to <code>domain.bind(callback)</code>.  However, in&#10;addition to catching thrown errors, it will also intercept <code>Error</code>&#10;objects sent as the first argument to the function.&#10;&#10;</p>&#10;<p>In this way, the common <code>if (er) return callback(er);</code> pattern can be replaced&#10;with a single error handler in a single place.&#10;&#10;</p>&#10;<h4>Example</h4>&#10;<pre><code>var d = domain.create();&#10;&#10;function readSomeFile(filename, cb) {&#10;  fs.readFile(filename, &#39;utf8&#39;, d.intercept(function(data) {&#10;    // note, the first argument is never passed to the&#10;    // callback since it is assumed to be the &#39;Error&#39; argument&#10;    // and thus intercepted by the domain.&#10;&#10;    // if this throws, it will also be passed to the domain&#10;    // so the error-handling logic can be moved to the &#39;error&#39;&#10;    // event on the domain instead of being repeated throughout&#10;    // the program.&#10;    return cb(null, JSON.parse(data));&#10;  }));&#10;}&#10;&#10;d.on(&#39;error&#39;, function(er) {&#10;  // an error occurred somewhere.&#10;  // if we throw it now, it will crash the program&#10;  // with the normal line number and stack message.&#10;});</code></pre>&#10;</summary>
                        /// <signature>
                        /// <param name="callback" type="Function">The callback function</param>
                        /// <returns type="Function">The intercepted function</returns>
                        /// </signature>
                        /// <signature>
                        /// <param name="callback"></param>
                        /// </signature>
                    }
                    this.enter = function() {
                        /// <summary><p>The <code>enter</code> method is plumbing used by the <code>run</code>, <code>bind</code>, and <code>intercept</code>&#10;methods to set the active domain. It sets <code>domain.active</code> and <code>process.domain</code>&#10;to the domain, and implicitly pushes the domain onto the domain stack managed&#10;by the domain module (see <code>domain.exit()</code> for details on the domain stack). The&#10;call to <code>enter</code> delimits the beginning of a chain of asynchronous calls and I/O&#10;operations bound to a domain.&#10;&#10;</p>&#10;<p>Calling <code>enter</code> changes only the active domain, and does not alter the domain&#10;itself. <code>Enter</code> and <code>exit</code> can be called an arbitrary number of times on a&#10;single domain.&#10;&#10;</p>&#10;<p>If the domain on which <code>enter</code> is called has been disposed, <code>enter</code> will return&#10;without setting the domain.&#10;&#10;</p>&#10;</summary>
                    }
                    this.exit = function() {
                        /// <summary><p>The <code>exit</code> method exits the current domain, popping it off the domain stack.&#10;Any time execution is going to switch to the context of a different chain of&#10;asynchronous calls, it&#39;s important to ensure that the current domain is exited.&#10;The call to <code>exit</code> delimits either the end of or an interruption to the chain&#10;of asynchronous calls and I/O operations bound to a domain.&#10;&#10;</p>&#10;<p>If there are multiple, nested domains bound to the current execution context,&#10;<code>exit</code> will exit any domains nested within this domain.&#10;&#10;</p>&#10;<p>Calling <code>exit</code> changes only the active domain, and does not alter the domain&#10;itself. <code>Enter</code> and <code>exit</code> can be called an arbitrary number of times on a&#10;single domain.&#10;&#10;</p>&#10;<p>If the domain on which <code>exit</code> is called has been disposed, <code>exit</code> will return&#10;without exiting the domain.&#10;&#10;</p>&#10;</summary>
                    }
                    this.dispose = function() {
                        /// <summary><p>The dispose method destroys a domain, and makes a best effort attempt to&#10;clean up any and all IO that is associated with the domain.  Streams are&#10;aborted, ended, closed, and/or destroyed.  Timers are cleared.&#10;Explicitly bound callbacks are no longer called.  Any error events that&#10;are raised as a result of this are ignored.&#10;&#10;</p>&#10;<p>The intention of calling <code>dispose</code> is generally to prevent cascading&#10;errors when a critical part of the Domain context is found to be in an&#10;error state.&#10;&#10;</p>&#10;<p>Once the domain is disposed the <code>dispose</code> event will emit.&#10;&#10;</p>&#10;<p>Note that IO might still be performed.  However, to the highest degree&#10;possible, once a domain is disposed, further errors from the emitters in&#10;that set will be ignored.  So, even if some remaining actions are still&#10;in flight, Node.js will not communicate further about them.&#10;&#10;</p>&#10;</summary>
                    }
                    /// <field name='members'><p>An array of timers and event emitters that have been explicitly added&#10;to the domain.&#10;&#10;</p>&#10;</field>
                    this.members = undefined;
                }

                this.Domain = function() {
                    return new _Domain();
                }
            };
            case "buffer": return new     function buffer() {
                /// <summary><p>Pure JavaScript is Unicode friendly but not nice to binary data.  When&#10;dealing with TCP streams or the file system, it&#39;s necessary to handle octet&#10;streams. Node has several strategies for manipulating, creating, and&#10;consuming octet streams.&#10;&#10;</p>&#10;<p>Raw data is stored in instances of the <code>Buffer</code> class. A <code>Buffer</code> is similar&#10;to an array of integers but corresponds to a raw memory allocation outside&#10;the V8 heap. A <code>Buffer</code> cannot be resized.&#10;&#10;</p>&#10;<p>The <code>Buffer</code> class is a global, making it very rare that one would need&#10;to ever <code>require(&#39;buffer&#39;)</code>.&#10;&#10;</p>&#10;<p>Converting between Buffers and JavaScript string objects requires an explicit&#10;encoding method.  Here are the different string encodings.&#10;&#10;</p>&#10;<ul>&#10;<li><p><code>&#39;ascii&#39;</code> - for 7 bit ASCII data only.  This encoding method is very fast, and&#10;will strip the high bit if set.</p>&#10;<p>Note that when converting from string to buffer, this encoding converts a null&#10;character (<code>&#39;\0&#39;</code> or <code>&#39;\u0000&#39;</code>) into <code>0x20</code> (character code of a space). If&#10;you want to convert a null character into <code>0x00</code>, you should use <code>&#39;utf8&#39;</code>.</p>&#10;</li>&#10;<li><p><code>&#39;utf8&#39;</code> - Multibyte encoded Unicode characters. Many web pages and other&#10;document formats use UTF-8.</p>&#10;</li>&#10;<li><p><code>&#39;utf16le&#39;</code> - 2 or 4 bytes, little endian encoded Unicode characters.&#10;Surrogate pairs (U+10000 to U+10FFFF) are supported.</p>&#10;</li>&#10;<li><p><code>&#39;ucs2&#39;</code> - Alias of <code>&#39;utf16le&#39;</code>.</p>&#10;</li>&#10;<li><p><code>&#39;base64&#39;</code> - Base64 string encoding.</p>&#10;</li>&#10;<li><p><code>&#39;binary&#39;</code> - A way of encoding raw binary data into strings by using only&#10;the first 8 bits of each character. This encoding method is deprecated and&#10;should be avoided in favor of <code>Buffer</code> objects where possible. This encoding&#10;will be removed in future versions of Node.</p>&#10;</li>&#10;<li><p><code>&#39;hex&#39;</code> - Encode each byte as two hexadecimal characters.</p>&#10;</li>&#10;</ul>&#10;<p>A <code>Buffer</code> object can also be used with typed arrays.  The buffer object is&#10;cloned to an <code>ArrayBuffer</code> that is used as the backing store for the typed&#10;array.  The memory of the buffer and the <code>ArrayBuffer</code> is not shared.&#10;&#10;</p>&#10;<p>NOTE: Node.js v0.8 simply retained a reference to the buffer in <code>array.buffer</code>&#10;instead of cloning it.&#10;&#10;</p>&#10;<p>While more efficient, it introduces subtle incompatibilities with the typed&#10;arrays specification.  <code>ArrayBuffer#slice()</code> makes a copy of the slice while&#10;<code>Buffer#slice()</code> creates a view.&#10;&#10;</p>&#10;</summary>
                function _Buffer() {
                    this.write = function(string, offset, length, encoding) {
                        /// <summary><p>Writes <code>string</code> to the buffer at <code>offset</code> using the given encoding.&#10;<code>offset</code> defaults to <code>0</code>, <code>encoding</code> defaults to <code>&#39;utf8&#39;</code>. <code>length</code> is&#10;the number of bytes to write. Returns number of octets written. If <code>buffer</code> did&#10;not contain enough space to fit the entire string, it will write a partial&#10;amount of the string. <code>length</code> defaults to <code>buffer.length - offset</code>.&#10;The method will not write partial characters.&#10;&#10;</p>&#10;<pre><code>buf = new Buffer(256);&#10;len = buf.write(&#39;\u00bd + \u00bc = \u00be&#39;, 0);&#10;console.log(len + &quot; bytes: &quot; + buf.toString(&#39;utf8&#39;, 0, len));</code></pre>&#10;<p>The number of characters written (which may be different than the number of&#10;bytes written) is set in <code>Buffer._charsWritten</code> and will be overwritten the&#10;next time <code>buf.write()</code> is called.&#10;&#10;&#10;</p>&#10;</summary>
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
                        /// <summary><p>Decodes and returns a string from buffer data encoded with <code>encoding</code>&#10;(defaults to <code>&#39;utf8&#39;</code>) beginning at <code>start</code> (defaults to <code>0</code>) and ending at&#10;<code>end</code> (defaults to <code>buffer.length</code>).&#10;&#10;</p>&#10;<p>See <code>buffer.write()</code> example, above.&#10;&#10;&#10;</p>&#10;</summary>
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
                    this.toJSON = function() {
                        /// <summary><p>Returns a JSON-representation of the Buffer instance, which is identical to the&#10;output for JSON Arrays. <code>JSON.stringify</code> implicitly calls this function when&#10;stringifying a Buffer instance.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>var buf = new Buffer(&#39;test&#39;);&#10;var json = JSON.stringify(buf);&#10;&#10;console.log(json);&#10;// &#39;[116,101,115,116]&#39;&#10;&#10;var copy = new Buffer(JSON.parse(json));&#10;&#10;console.log(copy);&#10;// &lt;Buffer 74 65 73 74&gt;</code></pre>&#10;</summary>
                    }
                    this.copy = function(targetBuffer, targetStart, sourceStart, sourceEnd) {
                        /// <summary><p>Does copy between buffers. The source and target regions can be overlapped.&#10;<code>targetStart</code> and <code>sourceStart</code> default to <code>0</code>.&#10;<code>sourceEnd</code> defaults to <code>buffer.length</code>.&#10;&#10;</p>&#10;<p>All values passed that are <code>undefined</code>/<code>NaN</code> or are out of bounds are set equal&#10;to their respective defaults.&#10;&#10;</p>&#10;<p>Example: build two Buffers, then copy <code>buf1</code> from byte 16 through byte 19&#10;into <code>buf2</code>, starting at the 8th byte in <code>buf2</code>.&#10;&#10;</p>&#10;<pre><code>buf1 = new Buffer(26);&#10;buf2 = new Buffer(26);&#10;&#10;for (var i = 0 ; i &lt; 26 ; i++) {&#10;  buf1[i] = i + 97; // 97 is ASCII a&#10;  buf2[i] = 33; // ASCII !&#10;}&#10;&#10;buf1.copy(buf2, 8, 16, 20);&#10;console.log(buf2.toString(&#39;ascii&#39;, 0, 25));&#10;&#10;// !!!!!!!!qrst!!!!!!!!!!!!!</code></pre>&#10;</summary>
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
                        /// <summary><p>Returns a new buffer which references the same memory as the old, but offset&#10;and cropped by the <code>start</code> (defaults to <code>0</code>) and <code>end</code> (defaults to&#10;<code>buffer.length</code>) indexes.  Negative indexes start from the end of the buffer.&#10;&#10;</p>&#10;<p><strong>Modifying the new buffer slice will modify memory in the original buffer!</strong>&#10;&#10;</p>&#10;<p>Example: build a Buffer with the ASCII alphabet, take a slice, then modify one&#10;byte from the original Buffer.&#10;&#10;</p>&#10;<pre><code>var buf1 = new Buffer(26);&#10;&#10;for (var i = 0 ; i &lt; 26 ; i++) {&#10;  buf1[i] = i + 97; // 97 is ASCII a&#10;}&#10;&#10;var buf2 = buf1.slice(0, 3);&#10;console.log(buf2.toString(&#39;ascii&#39;, 0, buf2.length));&#10;buf1[0] = 33;&#10;console.log(buf2.toString(&#39;ascii&#39;, 0, buf2.length));&#10;&#10;// abc&#10;// !bc</code></pre>&#10;</summary>
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
                        /// <summary><p>Reads an unsigned 8 bit integer from the buffer at the specified offset.&#10;&#10;</p>&#10;<p>Set <code>noAssert</code> to true to skip validation of <code>offset</code>. This means that <code>offset</code>&#10;may be beyond the end of the buffer. Defaults to <code>false</code>.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>var buf = new Buffer(4);&#10;&#10;buf[0] = 0x3;&#10;buf[1] = 0x4;&#10;buf[2] = 0x23;&#10;buf[3] = 0x42;&#10;&#10;for (ii = 0; ii &lt; buf.length; ii++) {&#10;  console.log(buf.readUInt8(ii));&#10;}&#10;&#10;// 0x3&#10;// 0x4&#10;// 0x23&#10;// 0x42</code></pre>&#10;</summary>
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
                        /// <summary><p>Reads an unsigned 16 bit integer from the buffer at the specified offset with&#10;specified endian format.&#10;&#10;</p>&#10;<p>Set <code>noAssert</code> to true to skip validation of <code>offset</code>. This means that <code>offset</code>&#10;may be beyond the end of the buffer. Defaults to <code>false</code>.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>var buf = new Buffer(4);&#10;&#10;buf[0] = 0x3;&#10;buf[1] = 0x4;&#10;buf[2] = 0x23;&#10;buf[3] = 0x42;&#10;&#10;console.log(buf.readUInt16BE(0));&#10;console.log(buf.readUInt16LE(0));&#10;console.log(buf.readUInt16BE(1));&#10;console.log(buf.readUInt16LE(1));&#10;console.log(buf.readUInt16BE(2));&#10;console.log(buf.readUInt16LE(2));&#10;&#10;// 0x0304&#10;// 0x0403&#10;// 0x0423&#10;// 0x2304&#10;// 0x2342&#10;// 0x4223</code></pre>&#10;</summary>
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
                        /// <summary><p>Reads an unsigned 16 bit integer from the buffer at the specified offset with&#10;specified endian format.&#10;&#10;</p>&#10;<p>Set <code>noAssert</code> to true to skip validation of <code>offset</code>. This means that <code>offset</code>&#10;may be beyond the end of the buffer. Defaults to <code>false</code>.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>var buf = new Buffer(4);&#10;&#10;buf[0] = 0x3;&#10;buf[1] = 0x4;&#10;buf[2] = 0x23;&#10;buf[3] = 0x42;&#10;&#10;console.log(buf.readUInt16BE(0));&#10;console.log(buf.readUInt16LE(0));&#10;console.log(buf.readUInt16BE(1));&#10;console.log(buf.readUInt16LE(1));&#10;console.log(buf.readUInt16BE(2));&#10;console.log(buf.readUInt16LE(2));&#10;&#10;// 0x0304&#10;// 0x0403&#10;// 0x0423&#10;// 0x2304&#10;// 0x2342&#10;// 0x4223</code></pre>&#10;</summary>
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
                        /// <summary><p>Reads an unsigned 32 bit integer from the buffer at the specified offset with&#10;specified endian format.&#10;&#10;</p>&#10;<p>Set <code>noAssert</code> to true to skip validation of <code>offset</code>. This means that <code>offset</code>&#10;may be beyond the end of the buffer. Defaults to <code>false</code>.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>var buf = new Buffer(4);&#10;&#10;buf[0] = 0x3;&#10;buf[1] = 0x4;&#10;buf[2] = 0x23;&#10;buf[3] = 0x42;&#10;&#10;console.log(buf.readUInt32BE(0));&#10;console.log(buf.readUInt32LE(0));&#10;&#10;// 0x03042342&#10;// 0x42230403</code></pre>&#10;</summary>
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
                        /// <summary><p>Reads an unsigned 32 bit integer from the buffer at the specified offset with&#10;specified endian format.&#10;&#10;</p>&#10;<p>Set <code>noAssert</code> to true to skip validation of <code>offset</code>. This means that <code>offset</code>&#10;may be beyond the end of the buffer. Defaults to <code>false</code>.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>var buf = new Buffer(4);&#10;&#10;buf[0] = 0x3;&#10;buf[1] = 0x4;&#10;buf[2] = 0x23;&#10;buf[3] = 0x42;&#10;&#10;console.log(buf.readUInt32BE(0));&#10;console.log(buf.readUInt32LE(0));&#10;&#10;// 0x03042342&#10;// 0x42230403</code></pre>&#10;</summary>
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
                        /// <summary><p>Reads a signed 8 bit integer from the buffer at the specified offset.&#10;&#10;</p>&#10;<p>Set <code>noAssert</code> to true to skip validation of <code>offset</code>. This means that <code>offset</code>&#10;may be beyond the end of the buffer. Defaults to <code>false</code>.&#10;&#10;</p>&#10;<p>Works as <code>buffer.readUInt8</code>, except buffer contents are treated as two&#39;s&#10;complement signed values.&#10;&#10;</p>&#10;</summary>
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
                        /// <summary><p>Reads a signed 16 bit integer from the buffer at the specified offset with&#10;specified endian format.&#10;&#10;</p>&#10;<p>Set <code>noAssert</code> to true to skip validation of <code>offset</code>. This means that <code>offset</code>&#10;may be beyond the end of the buffer. Defaults to <code>false</code>.&#10;&#10;</p>&#10;<p>Works as <code>buffer.readUInt16*</code>, except buffer contents are treated as two&#39;s&#10;complement signed values.&#10;&#10;</p>&#10;</summary>
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
                        /// <summary><p>Reads a signed 16 bit integer from the buffer at the specified offset with&#10;specified endian format.&#10;&#10;</p>&#10;<p>Set <code>noAssert</code> to true to skip validation of <code>offset</code>. This means that <code>offset</code>&#10;may be beyond the end of the buffer. Defaults to <code>false</code>.&#10;&#10;</p>&#10;<p>Works as <code>buffer.readUInt16*</code>, except buffer contents are treated as two&#39;s&#10;complement signed values.&#10;&#10;</p>&#10;</summary>
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
                        /// <summary><p>Reads a signed 32 bit integer from the buffer at the specified offset with&#10;specified endian format.&#10;&#10;</p>&#10;<p>Set <code>noAssert</code> to true to skip validation of <code>offset</code>. This means that <code>offset</code>&#10;may be beyond the end of the buffer. Defaults to <code>false</code>.&#10;&#10;</p>&#10;<p>Works as <code>buffer.readUInt32*</code>, except buffer contents are treated as two&#39;s&#10;complement signed values.&#10;&#10;</p>&#10;</summary>
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
                        /// <summary><p>Reads a signed 32 bit integer from the buffer at the specified offset with&#10;specified endian format.&#10;&#10;</p>&#10;<p>Set <code>noAssert</code> to true to skip validation of <code>offset</code>. This means that <code>offset</code>&#10;may be beyond the end of the buffer. Defaults to <code>false</code>.&#10;&#10;</p>&#10;<p>Works as <code>buffer.readUInt32*</code>, except buffer contents are treated as two&#39;s&#10;complement signed values.&#10;&#10;</p>&#10;</summary>
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
                        /// <summary><p>Reads a 32 bit float from the buffer at the specified offset with specified&#10;endian format.&#10;&#10;</p>&#10;<p>Set <code>noAssert</code> to true to skip validation of <code>offset</code>. This means that <code>offset</code>&#10;may be beyond the end of the buffer. Defaults to <code>false</code>.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>var buf = new Buffer(4);&#10;&#10;buf[0] = 0x00;&#10;buf[1] = 0x00;&#10;buf[2] = 0x80;&#10;buf[3] = 0x3f;&#10;&#10;console.log(buf.readFloatLE(0));&#10;&#10;// 0x01</code></pre>&#10;</summary>
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
                        /// <summary><p>Reads a 32 bit float from the buffer at the specified offset with specified&#10;endian format.&#10;&#10;</p>&#10;<p>Set <code>noAssert</code> to true to skip validation of <code>offset</code>. This means that <code>offset</code>&#10;may be beyond the end of the buffer. Defaults to <code>false</code>.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>var buf = new Buffer(4);&#10;&#10;buf[0] = 0x00;&#10;buf[1] = 0x00;&#10;buf[2] = 0x80;&#10;buf[3] = 0x3f;&#10;&#10;console.log(buf.readFloatLE(0));&#10;&#10;// 0x01</code></pre>&#10;</summary>
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
                        /// <summary><p>Reads a 64 bit double from the buffer at the specified offset with specified&#10;endian format.&#10;&#10;</p>&#10;<p>Set <code>noAssert</code> to true to skip validation of <code>offset</code>. This means that <code>offset</code>&#10;may be beyond the end of the buffer. Defaults to <code>false</code>.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>var buf = new Buffer(8);&#10;&#10;buf[0] = 0x55;&#10;buf[1] = 0x55;&#10;buf[2] = 0x55;&#10;buf[3] = 0x55;&#10;buf[4] = 0x55;&#10;buf[5] = 0x55;&#10;buf[6] = 0xd5;&#10;buf[7] = 0x3f;&#10;&#10;console.log(buf.readDoubleLE(0));&#10;&#10;// 0.3333333333333333</code></pre>&#10;</summary>
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
                        /// <summary><p>Reads a 64 bit double from the buffer at the specified offset with specified&#10;endian format.&#10;&#10;</p>&#10;<p>Set <code>noAssert</code> to true to skip validation of <code>offset</code>. This means that <code>offset</code>&#10;may be beyond the end of the buffer. Defaults to <code>false</code>.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>var buf = new Buffer(8);&#10;&#10;buf[0] = 0x55;&#10;buf[1] = 0x55;&#10;buf[2] = 0x55;&#10;buf[3] = 0x55;&#10;buf[4] = 0x55;&#10;buf[5] = 0x55;&#10;buf[6] = 0xd5;&#10;buf[7] = 0x3f;&#10;&#10;console.log(buf.readDoubleLE(0));&#10;&#10;// 0.3333333333333333</code></pre>&#10;</summary>
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
                        /// <summary><p>Writes <code>value</code> to the buffer at the specified offset. Note, <code>value</code> must be a&#10;valid unsigned 8 bit integer.&#10;&#10;</p>&#10;<p>Set <code>noAssert</code> to true to skip validation of <code>value</code> and <code>offset</code>. This means&#10;that <code>value</code> may be too large for the specific function and <code>offset</code> may be&#10;beyond the end of the buffer leading to the values being silently dropped. This&#10;should not be used unless you are certain of correctness. Defaults to <code>false</code>.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>var buf = new Buffer(4);&#10;buf.writeUInt8(0x3, 0);&#10;buf.writeUInt8(0x4, 1);&#10;buf.writeUInt8(0x23, 2);&#10;buf.writeUInt8(0x42, 3);&#10;&#10;console.log(buf);&#10;&#10;// &lt;Buffer 03 04 23 42&gt;</code></pre>&#10;</summary>
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
                        /// <summary><p>Writes <code>value</code> to the buffer at the specified offset with specified endian&#10;format. Note, <code>value</code> must be a valid unsigned 16 bit integer.&#10;&#10;</p>&#10;<p>Set <code>noAssert</code> to true to skip validation of <code>value</code> and <code>offset</code>. This means&#10;that <code>value</code> may be too large for the specific function and <code>offset</code> may be&#10;beyond the end of the buffer leading to the values being silently dropped. This&#10;should not be used unless you are certain of correctness. Defaults to <code>false</code>.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>var buf = new Buffer(4);&#10;buf.writeUInt16BE(0xdead, 0);&#10;buf.writeUInt16BE(0xbeef, 2);&#10;&#10;console.log(buf);&#10;&#10;buf.writeUInt16LE(0xdead, 0);&#10;buf.writeUInt16LE(0xbeef, 2);&#10;&#10;console.log(buf);&#10;&#10;// &lt;Buffer de ad be ef&gt;&#10;// &lt;Buffer ad de ef be&gt;</code></pre>&#10;</summary>
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
                        /// <summary><p>Writes <code>value</code> to the buffer at the specified offset with specified endian&#10;format. Note, <code>value</code> must be a valid unsigned 16 bit integer.&#10;&#10;</p>&#10;<p>Set <code>noAssert</code> to true to skip validation of <code>value</code> and <code>offset</code>. This means&#10;that <code>value</code> may be too large for the specific function and <code>offset</code> may be&#10;beyond the end of the buffer leading to the values being silently dropped. This&#10;should not be used unless you are certain of correctness. Defaults to <code>false</code>.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>var buf = new Buffer(4);&#10;buf.writeUInt16BE(0xdead, 0);&#10;buf.writeUInt16BE(0xbeef, 2);&#10;&#10;console.log(buf);&#10;&#10;buf.writeUInt16LE(0xdead, 0);&#10;buf.writeUInt16LE(0xbeef, 2);&#10;&#10;console.log(buf);&#10;&#10;// &lt;Buffer de ad be ef&gt;&#10;// &lt;Buffer ad de ef be&gt;</code></pre>&#10;</summary>
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
                        /// <summary><p>Writes <code>value</code> to the buffer at the specified offset with specified endian&#10;format. Note, <code>value</code> must be a valid unsigned 32 bit integer.&#10;&#10;</p>&#10;<p>Set <code>noAssert</code> to true to skip validation of <code>value</code> and <code>offset</code>. This means&#10;that <code>value</code> may be too large for the specific function and <code>offset</code> may be&#10;beyond the end of the buffer leading to the values being silently dropped. This&#10;should not be used unless you are certain of correctness. Defaults to <code>false</code>.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>var buf = new Buffer(4);&#10;buf.writeUInt32BE(0xfeedface, 0);&#10;&#10;console.log(buf);&#10;&#10;buf.writeUInt32LE(0xfeedface, 0);&#10;&#10;console.log(buf);&#10;&#10;// &lt;Buffer fe ed fa ce&gt;&#10;// &lt;Buffer ce fa ed fe&gt;</code></pre>&#10;</summary>
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
                        /// <summary><p>Writes <code>value</code> to the buffer at the specified offset with specified endian&#10;format. Note, <code>value</code> must be a valid unsigned 32 bit integer.&#10;&#10;</p>&#10;<p>Set <code>noAssert</code> to true to skip validation of <code>value</code> and <code>offset</code>. This means&#10;that <code>value</code> may be too large for the specific function and <code>offset</code> may be&#10;beyond the end of the buffer leading to the values being silently dropped. This&#10;should not be used unless you are certain of correctness. Defaults to <code>false</code>.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>var buf = new Buffer(4);&#10;buf.writeUInt32BE(0xfeedface, 0);&#10;&#10;console.log(buf);&#10;&#10;buf.writeUInt32LE(0xfeedface, 0);&#10;&#10;console.log(buf);&#10;&#10;// &lt;Buffer fe ed fa ce&gt;&#10;// &lt;Buffer ce fa ed fe&gt;</code></pre>&#10;</summary>
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
                        /// <summary><p>Writes <code>value</code> to the buffer at the specified offset. Note, <code>value</code> must be a&#10;valid signed 8 bit integer.&#10;&#10;</p>&#10;<p>Set <code>noAssert</code> to true to skip validation of <code>value</code> and <code>offset</code>. This means&#10;that <code>value</code> may be too large for the specific function and <code>offset</code> may be&#10;beyond the end of the buffer leading to the values being silently dropped. This&#10;should not be used unless you are certain of correctness. Defaults to <code>false</code>.&#10;&#10;</p>&#10;<p>Works as <code>buffer.writeUInt8</code>, except value is written out as a two&#39;s complement&#10;signed integer into <code>buffer</code>.&#10;&#10;</p>&#10;</summary>
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
                        /// <summary><p>Writes <code>value</code> to the buffer at the specified offset with specified endian&#10;format. Note, <code>value</code> must be a valid signed 16 bit integer.&#10;&#10;</p>&#10;<p>Set <code>noAssert</code> to true to skip validation of <code>value</code> and <code>offset</code>. This means&#10;that <code>value</code> may be too large for the specific function and <code>offset</code> may be&#10;beyond the end of the buffer leading to the values being silently dropped. This&#10;should not be used unless you are certain of correctness. Defaults to <code>false</code>.&#10;&#10;</p>&#10;<p>Works as <code>buffer.writeUInt16*</code>, except value is written out as a two&#39;s&#10;complement signed integer into <code>buffer</code>.&#10;&#10;</p>&#10;</summary>
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
                        /// <summary><p>Writes <code>value</code> to the buffer at the specified offset with specified endian&#10;format. Note, <code>value</code> must be a valid signed 16 bit integer.&#10;&#10;</p>&#10;<p>Set <code>noAssert</code> to true to skip validation of <code>value</code> and <code>offset</code>. This means&#10;that <code>value</code> may be too large for the specific function and <code>offset</code> may be&#10;beyond the end of the buffer leading to the values being silently dropped. This&#10;should not be used unless you are certain of correctness. Defaults to <code>false</code>.&#10;&#10;</p>&#10;<p>Works as <code>buffer.writeUInt16*</code>, except value is written out as a two&#39;s&#10;complement signed integer into <code>buffer</code>.&#10;&#10;</p>&#10;</summary>
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
                        /// <summary><p>Writes <code>value</code> to the buffer at the specified offset with specified endian&#10;format. Note, <code>value</code> must be a valid signed 32 bit integer.&#10;&#10;</p>&#10;<p>Set <code>noAssert</code> to true to skip validation of <code>value</code> and <code>offset</code>. This means&#10;that <code>value</code> may be too large for the specific function and <code>offset</code> may be&#10;beyond the end of the buffer leading to the values being silently dropped. This&#10;should not be used unless you are certain of correctness. Defaults to <code>false</code>.&#10;&#10;</p>&#10;<p>Works as <code>buffer.writeUInt32*</code>, except value is written out as a two&#39;s&#10;complement signed integer into <code>buffer</code>.&#10;&#10;</p>&#10;</summary>
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
                        /// <summary><p>Writes <code>value</code> to the buffer at the specified offset with specified endian&#10;format. Note, <code>value</code> must be a valid signed 32 bit integer.&#10;&#10;</p>&#10;<p>Set <code>noAssert</code> to true to skip validation of <code>value</code> and <code>offset</code>. This means&#10;that <code>value</code> may be too large for the specific function and <code>offset</code> may be&#10;beyond the end of the buffer leading to the values being silently dropped. This&#10;should not be used unless you are certain of correctness. Defaults to <code>false</code>.&#10;&#10;</p>&#10;<p>Works as <code>buffer.writeUInt32*</code>, except value is written out as a two&#39;s&#10;complement signed integer into <code>buffer</code>.&#10;&#10;</p>&#10;</summary>
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
                        /// <summary><p>Writes <code>value</code> to the buffer at the specified offset with specified endian&#10;format. Note, behavior is unspecified if <code>value</code> is not a 32 bit float.&#10;&#10;</p>&#10;<p>Set <code>noAssert</code> to true to skip validation of <code>value</code> and <code>offset</code>. This means&#10;that <code>value</code> may be too large for the specific function and <code>offset</code> may be&#10;beyond the end of the buffer leading to the values being silently dropped. This&#10;should not be used unless you are certain of correctness. Defaults to <code>false</code>.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>var buf = new Buffer(4);&#10;buf.writeFloatBE(0xcafebabe, 0);&#10;&#10;console.log(buf);&#10;&#10;buf.writeFloatLE(0xcafebabe, 0);&#10;&#10;console.log(buf);&#10;&#10;// &lt;Buffer 4f 4a fe bb&gt;&#10;// &lt;Buffer bb fe 4a 4f&gt;</code></pre>&#10;</summary>
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
                        /// <summary><p>Writes <code>value</code> to the buffer at the specified offset with specified endian&#10;format. Note, behavior is unspecified if <code>value</code> is not a 32 bit float.&#10;&#10;</p>&#10;<p>Set <code>noAssert</code> to true to skip validation of <code>value</code> and <code>offset</code>. This means&#10;that <code>value</code> may be too large for the specific function and <code>offset</code> may be&#10;beyond the end of the buffer leading to the values being silently dropped. This&#10;should not be used unless you are certain of correctness. Defaults to <code>false</code>.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>var buf = new Buffer(4);&#10;buf.writeFloatBE(0xcafebabe, 0);&#10;&#10;console.log(buf);&#10;&#10;buf.writeFloatLE(0xcafebabe, 0);&#10;&#10;console.log(buf);&#10;&#10;// &lt;Buffer 4f 4a fe bb&gt;&#10;// &lt;Buffer bb fe 4a 4f&gt;</code></pre>&#10;</summary>
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
                        /// <summary><p>Writes <code>value</code> to the buffer at the specified offset with specified endian&#10;format. Note, <code>value</code> must be a valid 64 bit double.&#10;&#10;</p>&#10;<p>Set <code>noAssert</code> to true to skip validation of <code>value</code> and <code>offset</code>. This means&#10;that <code>value</code> may be too large for the specific function and <code>offset</code> may be&#10;beyond the end of the buffer leading to the values being silently dropped. This&#10;should not be used unless you are certain of correctness. Defaults to <code>false</code>.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>var buf = new Buffer(8);&#10;buf.writeDoubleBE(0xdeadbeefcafebabe, 0);&#10;&#10;console.log(buf);&#10;&#10;buf.writeDoubleLE(0xdeadbeefcafebabe, 0);&#10;&#10;console.log(buf);&#10;&#10;// &lt;Buffer 43 eb d5 b7 dd f9 5f d7&gt;&#10;// &lt;Buffer d7 5f f9 dd b7 d5 eb 43&gt;</code></pre>&#10;</summary>
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
                        /// <summary><p>Writes <code>value</code> to the buffer at the specified offset with specified endian&#10;format. Note, <code>value</code> must be a valid 64 bit double.&#10;&#10;</p>&#10;<p>Set <code>noAssert</code> to true to skip validation of <code>value</code> and <code>offset</code>. This means&#10;that <code>value</code> may be too large for the specific function and <code>offset</code> may be&#10;beyond the end of the buffer leading to the values being silently dropped. This&#10;should not be used unless you are certain of correctness. Defaults to <code>false</code>.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>var buf = new Buffer(8);&#10;buf.writeDoubleBE(0xdeadbeefcafebabe, 0);&#10;&#10;console.log(buf);&#10;&#10;buf.writeDoubleLE(0xdeadbeefcafebabe, 0);&#10;&#10;console.log(buf);&#10;&#10;// &lt;Buffer 43 eb d5 b7 dd f9 5f d7&gt;&#10;// &lt;Buffer d7 5f f9 dd b7 d5 eb 43&gt;</code></pre>&#10;</summary>
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
                        /// <summary><p>Fills the buffer with the specified value. If the <code>offset</code> (defaults to <code>0</code>)&#10;and <code>end</code> (defaults to <code>buffer.length</code>) are not given it will fill the entire&#10;buffer.&#10;&#10;</p>&#10;<pre><code>var b = new Buffer(50);&#10;b.fill(&quot;h&quot;);</code></pre>&#10;</summary>
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
                    /// <field name='[index]'><p>Get and set the octet at <code>index</code>. The values refer to individual bytes,&#10;so the legal range is between <code>0x00</code> and <code>0xFF</code> hex or <code>0</code> and <code>255</code>.&#10;&#10;</p>&#10;<p>Example: copy an ASCII string into a buffer, one byte at a time:&#10;&#10;</p>&#10;<pre><code>str = &quot;node.js&quot;;&#10;buf = new Buffer(str.length);&#10;&#10;for (var i = 0; i &lt; str.length ; i++) {&#10;  buf[i] = str.charCodeAt(i);&#10;}&#10;&#10;console.log(buf);&#10;&#10;// node.js</code></pre>&#10;</field>
                    this.[index] = undefined;
                    /// <field name='length'><p>The size of the buffer in bytes.  Note that this is not necessarily the size&#10;of the contents. <code>length</code> refers to the amount of memory allocated for the&#10;buffer object.  It does not change when the contents of the buffer are changed.&#10;&#10;</p>&#10;<pre><code>buf = new Buffer(1234);&#10;&#10;console.log(buf.length);&#10;buf.write(&quot;some string&quot;, 0, &quot;ascii&quot;);&#10;console.log(buf.length);&#10;&#10;// 1234&#10;// 1234</code></pre>&#10;</field>
                    this.length = undefined;
                }

                this.Buffer = function() {
                    return new _Buffer();
                }
                function _SlowBuffer() {
                }

                this.SlowBuffer = function() {
                    return new _SlowBuffer();
                }
                /// <field name='INSPECT_MAX_BYTES'><p>How many bytes will be returned when <code>buffer.inspect()</code> is called. This can&#10;be overridden by user modules.&#10;&#10;</p>&#10;<p>Note that this is a property on the buffer module returned by&#10;<code>require(&#39;buffer&#39;)</code>, not on the Buffer global, or a buffer instance.&#10;&#10;</p>&#10;</field>
                this.INSPECT_MAX_BYTES = undefined;
            };
            case "stream": return new     function stream() {
                /// <summary><p>A stream is an abstract interface implemented by various objects in&#10;Node.  For example a <a href="http.html#http_http_incomingmessage">request to an HTTP&#10;server</a> is a stream, as is&#10;[stdout][]. Streams are readable, writable, or both. All streams are&#10;instances of [EventEmitter][]&#10;&#10;</p>&#10;<p>You can load the Stream base classes by doing <code>require(&#39;stream&#39;)</code>.&#10;There are base classes provided for [Readable][] streams, [Writable][]&#10;streams, [Duplex][] streams, and [Transform][] streams.&#10;&#10;</p>&#10;<p>This document is split up into 3 sections.  The first explains the&#10;parts of the API that you need to be aware of to use streams in your&#10;programs.  If you never implement a streaming API yourself, you can&#10;stop there.&#10;&#10;</p>&#10;<p>The second section explains the parts of the API that you need to use&#10;if you implement your own custom streams yourself.  The API is&#10;designed to make this easy for you to do.&#10;&#10;</p>&#10;<p>The third section goes into more depth about how streams work,&#10;including some of the internal mechanisms and functions that you&#10;should probably not modify unless you definitely know what you are&#10;doing.&#10;&#10;&#10;</p>&#10;</summary>
                function _Readable() {
                    this.read = function(size) {
                        /// <summary><p>The <code>read()</code> method pulls some data out of the internal buffer and&#10;returns it.  If there is no data available, then it will return&#10;<code>null</code>.&#10;&#10;</p>&#10;<p>If you pass in a <code>size</code> argument, then it will return that many&#10;bytes.  If <code>size</code> bytes are not available, then it will return <code>null</code>.&#10;&#10;</p>&#10;<p>If you do not specify a <code>size</code> argument, then it will return all the&#10;data in the internal buffer.&#10;&#10;</p>&#10;<p>This method should only be called in non-flowing mode.  In&#10;flowing-mode, this method is called automatically until the internal&#10;buffer is drained.&#10;&#10;</p>&#10;<pre><code class="javascript">var readable = getReadableStreamSomehow();&#10;readable.on(&#39;readable&#39;, function() {&#10;  var chunk;&#10;  while (null !== (chunk = readable.read())) {&#10;    console.log(&#39;got %d bytes of data&#39;, chunk.length);&#10;  }&#10;});</code></pre>&#10;</summary>
                        /// <signature>
                        /// <param name="size" type="Number">Optional argument to specify how much data to read.</param>
                        /// <returns type="String"></returns>
                        /// </signature>
                        /// <signature>
                        /// <param name="size"></param>
                        /// </signature>
                    }
                    this.setEncoding = function(encoding) {
                        /// <summary><p>Call this function to cause the stream to return strings of the&#10;specified encoding instead of Buffer objects.  For example, if you do&#10;<code>readable.setEncoding(&#39;utf8&#39;)</code>, then the output data will be&#10;interpreted as UTF-8 data, and returned as strings.  If you do&#10;<code>readable.setEncoding(&#39;hex&#39;)</code>, then the data will be encoded in&#10;hexadecimal string format.&#10;&#10;</p>&#10;<p>This properly handles multi-byte characters that would otherwise be&#10;potentially mangled if you simply pulled the Buffers directly and&#10;called <code>buf.toString(encoding)</code> on them.  If you want to read the data&#10;as strings, always use this method.&#10;&#10;</p>&#10;<pre><code class="javascript">var readable = getReadableStreamSomehow();&#10;readable.setEncoding(&#39;utf8&#39;);&#10;readable.on(&#39;data&#39;, function(chunk) {&#10;  assert.equal(typeof chunk, &#39;string&#39;);&#10;  console.log(&#39;got %d characters of string data&#39;, chunk.length);&#10;})</code></pre>&#10;</summary>
                        /// <signature>
                        /// <param name="encoding" type="String">The encoding to use.</param>
                        /// </signature>
                        /// <signature>
                        /// <param name="encoding"></param>
                        /// </signature>
                    }
                    this.resume = function() {
                        /// <summary><p>This method will cause the readable stream to resume emitting <code>data</code>&#10;events.&#10;&#10;</p>&#10;<p>This method will switch the stream into flowing-mode.  If you do <em>not</em>&#10;want to consume the data from a stream, but you <em>do</em> want to get to&#10;its <code>end</code> event, you can call <code>readable.resume()</code> to open the flow of&#10;data.&#10;&#10;</p>&#10;<pre><code class="javascript">var readable = getReadableStreamSomehow();&#10;readable.resume();&#10;readable.on(&#39;end&#39;, function(chunk) {&#10;  console.log(&#39;got to the end, but did not read anything&#39;);&#10;})</code></pre>&#10;</summary>
                    }
                    this.pause = function() {
                        /// <summary><p>This method will cause a stream in flowing-mode to stop emitting&#10;<code>data</code> events.  Any data that becomes available will remain in the&#10;internal buffer.&#10;&#10;</p>&#10;<p>This method is only relevant in flowing mode.  When called on a&#10;non-flowing stream, it will switch into flowing mode, but remain&#10;paused.&#10;&#10;</p>&#10;<pre><code class="javascript">var readable = getReadableStreamSomehow();&#10;readable.on(&#39;data&#39;, function(chunk) {&#10;  console.log(&#39;got %d bytes of data&#39;, chunk.length);&#10;  readable.pause();&#10;  console.log(&#39;there will be no more data for 1 second&#39;);&#10;  setTimeout(function() {&#10;    console.log(&#39;now data will start flowing again&#39;);&#10;    readable.resume();&#10;  }, 1000);&#10;})</code></pre>&#10;</summary>
                    }
                    this.pipe = function(destination, options) {
                        /// <summary><p>This method pulls all the data out of a readable stream, and writes it&#10;to the supplied destination, automatically managing the flow so that&#10;the destination is not overwhelmed by a fast readable stream.&#10;&#10;</p>&#10;<p>Multiple destinations can be piped to safely.&#10;&#10;</p>&#10;<pre><code class="javascript">var readable = getReadableStreamSomehow();&#10;var writable = fs.createWriteStream(&#39;file.txt&#39;);&#10;// All the data from readable goes into &#39;file.txt&#39;&#10;readable.pipe(writable);</code></pre>&#10;<p>This function returns the destination stream, so you can set up pipe&#10;chains like so:&#10;&#10;</p>&#10;<pre><code class="javascript">var r = fs.createReadStream(&#39;file.txt&#39;);&#10;var z = zlib.createGzip();&#10;var w = fs.createWriteStream(&#39;file.txt.gz&#39;);&#10;r.pipe(z).pipe(w);</code></pre>&#10;<p>For example, emulating the Unix <code>cat</code> command:&#10;&#10;</p>&#10;<pre><code class="javascript">process.stdin.pipe(process.stdout);</code></pre>&#10;<p>By default [<code>end()</code>][] is called on the destination when the source stream&#10;emits <code>end</code>, so that <code>destination</code> is no longer writable. Pass <code>{ end:&#10;false }</code> as <code>options</code> to keep the destination stream open.&#10;&#10;</p>&#10;<p>This keeps <code>writer</code> open so that &quot;Goodbye&quot; can be written at the&#10;end.&#10;&#10;</p>&#10;<pre><code class="javascript">reader.pipe(writer, { end: false });&#10;reader.on(&#39;end&#39;, function() {&#10;  writer.end(&#39;Goodbye\n&#39;);&#10;});</code></pre>&#10;<p>Note that <code>process.stderr</code> and <code>process.stdout</code> are never closed until&#10;the process exits, regardless of the specified options.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="destination" type="">The destination for writing data</param>
                        /// <param name="options" type="Object">Pipe options</param>
                        /// </signature>
                        /// <signature>
                        /// <param name="destination"></param>
                        /// <param name="options"></param>
                        /// </signature>
                    }
                    this.unpipe = function(destination) {
                        /// <summary><p>This method will remove the hooks set up for a previous <code>pipe()</code> call.&#10;&#10;</p>&#10;<p>If the destination is not specified, then all pipes are removed.&#10;&#10;</p>&#10;<p>If the destination is specified, but no pipe is set up for it, then&#10;this is a no-op.&#10;&#10;</p>&#10;<pre><code class="javascript">var readable = getReadableStreamSomehow();&#10;var writable = fs.createWriteStream(&#39;file.txt&#39;);&#10;// All the data from readable goes into &#39;file.txt&#39;,&#10;// but only for the first second&#10;readable.pipe(writable);&#10;setTimeout(function() {&#10;  console.log(&#39;stop writing to file.txt&#39;);&#10;  readable.unpipe(writable);&#10;  console.log(&#39;manually close the file stream&#39;);&#10;  writable.end();&#10;}, 1000);</code></pre>&#10;</summary>
                        /// <signature>
                        /// <param name="destination" type="">Optional specific stream to unpipe</param>
                        /// </signature>
                        /// <signature>
                        /// <param name="destination"></param>
                        /// </signature>
                    }
                    this.unshift = function(chunk) {
                        /// <summary><p>This is useful in certain cases where a stream is being consumed by a&#10;parser, which needs to &quot;un-consume&quot; some data that it has&#10;optimistically pulled out of the source, so that the stream can be&#10;passed on to some other party.&#10;&#10;</p>&#10;<p>If you find that you must often call <code>stream.unshift(chunk)</code> in your&#10;programs, consider implementing a [Transform][] stream instead.  (See API&#10;for Stream Implementors, below.)&#10;&#10;</p>&#10;<pre><code class="javascript">// Pull off a header delimited by \n\n&#10;// use unshift() if we get too much&#10;// Call the callback with (error, header, stream)&#10;var StringDecoder = require(&#39;string_decoder&#39;).StringDecoder;&#10;function parseHeader(stream, callback) {&#10;  stream.on(&#39;error&#39;, callback);&#10;  stream.on(&#39;readable&#39;, onReadable);&#10;  var decoder = new StringDecoder(&#39;utf8&#39;);&#10;  var header = &#39;&#39;;&#10;  function onReadable() {&#10;    var chunk;&#10;    while (null !== (chunk = stream.read())) {&#10;      var str = decoder.write(chunk);&#10;      if (str.match(/\n\n/)) {&#10;        // found the header boundary&#10;        var split = str.split(/\n\n/);&#10;        header += split.shift();&#10;        var remaining = split.join(&#39;\n\n&#39;);&#10;        var buf = new Buffer(remaining, &#39;utf8&#39;);&#10;        if (buf.length)&#10;          stream.unshift(buf);&#10;        stream.removeListener(&#39;error&#39;, callback);&#10;        stream.removeListener(&#39;readable&#39;, onReadable);&#10;        // now the body of the message can be read from the stream.&#10;        callback(null, header, stream);&#10;      } else {&#10;        // still reading the header.&#10;        header += str;&#10;      }&#10;    }&#10;  }&#10;}</code></pre>&#10;</summary>
                        /// <signature>
                        /// <param name="chunk" type="Buffer">Chunk of data to unshift onto the read queue</param>
                        /// </signature>
                        /// <signature>
                        /// <param name="chunk"></param>
                        /// </signature>
                    }
                    this.wrap = function(stream) {
                        /// <summary><p>Versions of Node prior to v0.10 had streams that did not implement the&#10;entire Streams API as it is today.  (See &quot;Compatibility&quot; below for&#10;more information.)&#10;&#10;</p>&#10;<p>If you are using an older Node library that emits <code>&#39;data&#39;</code> events and&#10;has a <code>pause()</code> method that is advisory only, then you can use the&#10;<code>wrap()</code> method to create a [Readable][] stream that uses the old stream&#10;as its data source.&#10;&#10;</p>&#10;<p>You will very rarely ever need to call this function, but it exists&#10;as a convenience for interacting with old Node programs and libraries.&#10;&#10;</p>&#10;<p>For example:&#10;&#10;</p>&#10;<pre><code class="javascript">var OldReader = require(&#39;./old-api-module.js&#39;).OldReader;&#10;var oreader = new OldReader;&#10;var Readable = require(&#39;stream&#39;).Readable;&#10;var myReader = new Readable().wrap(oreader);&#10;&#10;myReader.on(&#39;readable&#39;, function() {&#10;  myReader.read(); // etc.&#10;});</code></pre>&#10;</summary>
                        /// <signature>
                        /// <param name="stream" type="Stream">An "old style" readable stream</param>
                        /// </signature>
                        /// <signature>
                        /// <param name="stream"></param>
                        /// </signature>
                    }
                    this.addListener = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// readable: When a chunk of data can be read from the stream, it will emit a ...&#10;
                        /// data: If you attach a <code>data</code> event listener, then it will switch the stream ...&#10;
                        /// end: This event fires when no more data will be provided. ...&#10;
                        /// close: Emitted when the underlying resource (for example, the backing file ...&#10;
                        /// error: Emitted if there was an error receiving data. ...&#10;
                        /// </summary>

                    }
                    this.once = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// readable: When a chunk of data can be read from the stream, it will emit a ...&#10;
                        /// data: If you attach a <code>data</code> event listener, then it will switch the stream ...&#10;
                        /// end: This event fires when no more data will be provided. ...&#10;
                        /// close: Emitted when the underlying resource (for example, the backing file ...&#10;
                        /// error: Emitted if there was an error receiving data. ...&#10;
                        /// </summary>

                    }
                    this.removeListener = function(event, listener) {
                        /// <summary>Supported Events: readable, data, end, close, error</summary>
                    }
                    this.removeAllListeners = function(event) {
                        /// <summary>Supported Events: readable, data, end, close, error</summary>
                    }
                    this.setMaxListeners = function(n) { }
                    this.listeners = function(event) {
                        /// <summary>Supported Events: readable, data, end, close, error</summary>
                    }
                    this.emit = function(event, arguments) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// readable: When a chunk of data can be read from the stream, it will emit a ...&#10;
                        /// data: If you attach a <code>data</code> event listener, then it will switch the stream ...&#10;
                        /// end: This event fires when no more data will be provided. ...&#10;
                        /// close: Emitted when the underlying resource (for example, the backing file ...&#10;
                        /// error: Emitted if there was an error receiving data. ...&#10;
                        /// </summary>

                    }
                    this.on = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// readable: When a chunk of data can be read from the stream, it will emit a ...&#10;
                        /// data: If you attach a <code>data</code> event listener, then it will switch the stream ...&#10;
                        /// end: This event fires when no more data will be provided. ...&#10;
                        /// close: Emitted when the underlying resource (for example, the backing file ...&#10;
                        /// error: Emitted if there was an error receiving data. ...&#10;
                        /// </summary>

                    }
                }

                this.Readable = function() {
                    return new _Readable();
                }
                function _Writable() {
                    this.write = function(chunk, encoding, callback) {
                        /// <summary><p>This method writes some data to the underlying system, and calls the&#10;supplied callback once the data has been fully handled.&#10;&#10;</p>&#10;<p>The return value indicates if you should continue writing right now.&#10;If the data had to be buffered internally, then it will return&#10;<code>false</code>.  Otherwise, it will return <code>true</code>.&#10;&#10;</p>&#10;<p>This return value is strictly advisory.  You MAY continue to write,&#10;even if it returns <code>false</code>.  However, writes will be buffered in&#10;memory, so it is best not to do this excessively.  Instead, wait for&#10;the <code>drain</code> event before writing more data.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="chunk" type="String">The data to write</param>
                        /// <param name="encoding" type="String">The encoding, if `chunk` is a String</param>
                        /// <param name="callback" type="Function">Callback for when this chunk of data is flushed</param>
                        /// <returns type="Boolean">True if the data was handled completely.</returns>
                        /// </signature>
                        /// <signature>
                        /// <param name="chunk"></param>
                        /// <param name="encoding"></param>
                        /// <param name="callback"></param>
                        /// </signature>
                    }
                    this.end = function(chunk, encoding, callback) {
                        /// <summary><p>Call this method when no more data will be written to the stream.  If&#10;supplied, the callback is attached as a listener on the <code>finish</code> event.&#10;&#10;</p>&#10;<p>Calling [<code>write()</code>][] after calling [<code>end()</code>][] will raise an error.&#10;&#10;</p>&#10;<pre><code class="javascript">// write &#39;hello, &#39; and then end with &#39;world!&#39;&#10;http.createServer(function (req, res) {&#10;  res.write(&#39;hello, &#39;);&#10;  res.end(&#39;world!&#39;);&#10;  // writing more now is not allowed!&#10;});</code></pre>&#10;</summary>
                        /// <signature>
                        /// <param name="chunk" type="String">Optional data to write</param>
                        /// <param name="encoding" type="String">The encoding, if `chunk` is a String</param>
                        /// <param name="callback" type="Function">Optional callback for when the stream is finished</param>
                        /// </signature>
                        /// <signature>
                        /// <param name="chunk"></param>
                        /// <param name="encoding"></param>
                        /// <param name="callback"></param>
                        /// </signature>
                    }
                    this.addListener = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// drain: If a [<code>writable.write(chunk)</code>][] call returns false, then the <code>drain</code> ...&#10;
                        /// finish: When the [<code>end()</code>][] method has been called, and all data has been flushed ...&#10;
                        /// pipe: This is emitted whenever the <code>pipe()</code> method is called on a readable ...&#10;
                        /// unpipe: This is emitted whenever the [<code>unpipe()</code>][] method is called on a ...&#10;
                        /// </summary>

                    }
                    this.once = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// drain: If a [<code>writable.write(chunk)</code>][] call returns false, then the <code>drain</code> ...&#10;
                        /// finish: When the [<code>end()</code>][] method has been called, and all data has been flushed ...&#10;
                        /// pipe: This is emitted whenever the <code>pipe()</code> method is called on a readable ...&#10;
                        /// unpipe: This is emitted whenever the [<code>unpipe()</code>][] method is called on a ...&#10;
                        /// </summary>

                    }
                    this.removeListener = function(event, listener) {
                        /// <summary>Supported Events: drain, finish, pipe, unpipe</summary>
                    }
                    this.removeAllListeners = function(event) {
                        /// <summary>Supported Events: drain, finish, pipe, unpipe</summary>
                    }
                    this.setMaxListeners = function(n) { }
                    this.listeners = function(event) {
                        /// <summary>Supported Events: drain, finish, pipe, unpipe</summary>
                    }
                    this.emit = function(event, arguments) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// drain: If a [<code>writable.write(chunk)</code>][] call returns false, then the <code>drain</code> ...&#10;
                        /// finish: When the [<code>end()</code>][] method has been called, and all data has been flushed ...&#10;
                        /// pipe: This is emitted whenever the <code>pipe()</code> method is called on a readable ...&#10;
                        /// unpipe: This is emitted whenever the [<code>unpipe()</code>][] method is called on a ...&#10;
                        /// </summary>

                    }
                    this.on = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// drain: If a [<code>writable.write(chunk)</code>][] call returns false, then the <code>drain</code> ...&#10;
                        /// finish: When the [<code>end()</code>][] method has been called, and all data has been flushed ...&#10;
                        /// pipe: This is emitted whenever the <code>pipe()</code> method is called on a readable ...&#10;
                        /// unpipe: This is emitted whenever the [<code>unpipe()</code>][] method is called on a ...&#10;
                        /// </summary>

                    }
                }

                this.Writable = function() {
                    return new _Writable();
                }
                function _Duplex() {
                }

                this.Duplex = function() {
                    return new _Duplex();
                }
                function _Transform() {
                }

                this.Transform = function() {
                    return new _Transform();
                }
            };
            case "crypto": return new     function crypto() {
                /// <summary><pre><code>Stability: 2 - Unstable; API changes are being discussed for&#10;future versions.  Breaking changes will be minimized.  See below.</code></pre>&#10;<p>Use <code>require(&#39;crypto&#39;)</code> to access this module.&#10;&#10;</p>&#10;<p>The crypto module offers a way of encapsulating secure credentials to be&#10;used as part of a secure HTTPS net or http connection.&#10;&#10;</p>&#10;<p>It also offers a set of wrappers for OpenSSL&#39;s hash, hmac, cipher,&#10;decipher, sign and verify methods.&#10;&#10;&#10;</p>&#10;</summary>
                this.getCiphers = function() {
                    /// <summary><p>Returns an array with the names of the supported ciphers.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>var ciphers = crypto.getCiphers();&#10;console.log(ciphers); // [&#39;AES-128-CBC&#39;, &#39;AES-128-CBC-HMAC-SHA1&#39;, ...]</code></pre>&#10;</summary>
                }
                this.getHashes = function() {
                    /// <summary><p>Returns an array with the names of the supported hash algorithms.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>var hashes = crypto.getHashes();&#10;console.log(hashes); // [&#39;sha&#39;, &#39;sha1&#39;, &#39;sha1WithRSAEncryption&#39;, ...]</code></pre>&#10;</summary>
                }
                this.createCredentials = function(details) {
                    /// <summary><p>Creates a credentials object, with the optional details being a&#10;dictionary with keys:&#10;&#10;</p>&#10;<ul>&#10;<li><code>pfx</code> : A string or buffer holding the PFX or PKCS12 encoded private&#10;key, certificate and CA certificates</li>&#10;<li><code>key</code> : A string holding the PEM encoded private key</li>&#10;<li><code>passphrase</code> : A string of passphrase for the private key or pfx</li>&#10;<li><code>cert</code> : A string holding the PEM encoded certificate</li>&#10;<li><code>ca</code> : Either a string or list of strings of PEM encoded CA&#10;certificates to trust.</li>&#10;<li><code>crl</code> : Either a string or list of strings of PEM encoded CRLs&#10;(Certificate Revocation List)</li>&#10;<li><code>ciphers</code>: A string describing the ciphers to use or exclude.&#10;Consult&#10;<a href="http://www.openssl.org/docs/apps/ciphers.html#CIPHER_LIST_FORMAT">http://www.openssl.org/docs/apps/ciphers.html#CIPHER_LIST_FORMAT</a>&#10;for details on the format.</li>&#10;</ul>&#10;<p>If no &#39;ca&#39; details are given, then node.js will use the default&#10;publicly trusted list of CAs as given in&#10;</p>&#10;<p><a href="http://mxr.mozilla.org/mozilla/source/security/nss/lib/ckfw/builtins/certdata.txt">http://mxr.mozilla.org/mozilla/source/security/nss/lib/ckfw/builtins/certdata.txt</a>.&#10;&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="details"></param>
                    /// </signature>
                    return new this.Credentials();
                }
                this.createHash = function(algorithm) {
                    /// <summary><p>Creates and returns a hash object, a cryptographic hash with the given&#10;algorithm which can be used to generate hash digests.&#10;&#10;</p>&#10;<p><code>algorithm</code> is dependent on the available algorithms supported by the&#10;version of OpenSSL on the platform. Examples are <code>&#39;sha1&#39;</code>, <code>&#39;md5&#39;</code>,&#10;<code>&#39;sha256&#39;</code>, <code>&#39;sha512&#39;</code>, etc.  On recent releases, <code>openssl&#10;list-message-digest-algorithms</code> will display the available digest&#10;algorithms.&#10;&#10;</p>&#10;<p>Example: this program that takes the sha1 sum of a file&#10;&#10;</p>&#10;<pre><code>var filename = process.argv[2];&#10;var crypto = require(&#39;crypto&#39;);&#10;var fs = require(&#39;fs&#39;);&#10;&#10;var shasum = crypto.createHash(&#39;sha1&#39;);&#10;&#10;var s = fs.ReadStream(filename);&#10;s.on(&#39;data&#39;, function(d) {&#10;  shasum.update(d);&#10;});&#10;&#10;s.on(&#39;end&#39;, function() {&#10;  var d = shasum.digest(&#39;hex&#39;);&#10;  console.log(d + &#39;  &#39; + filename);&#10;});</code></pre>&#10;</summary>
                    /// <signature>
                    /// <param name="algorithm"></param>
                    /// </signature>
                    return new this.Hash();
                }
                this.createHmac = function(algorithm, key) {
                    /// <summary><p>Creates and returns a hmac object, a cryptographic hmac with the given&#10;algorithm and key.&#10;&#10;</p>&#10;<p>It is a <a href="stream.html">stream</a> that is both readable and writable.  The&#10;written data is used to compute the hmac.  Once the writable side of&#10;the stream is ended, use the <code>read()</code> method to get the computed&#10;digest.  The legacy <code>update</code> and <code>digest</code> methods are also supported.&#10;&#10;</p>&#10;<p><code>algorithm</code> is dependent on the available algorithms supported by&#10;OpenSSL - see createHash above.  <code>key</code> is the hmac key to be used.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="algorithm"></param>
                    /// <param name="key"></param>
                    /// </signature>
                    return new this.Hmac();
                }
                this.createCipher = function(algorithm, password) {
                    /// <summary><p>Creates and returns a cipher object, with the given algorithm and&#10;password.&#10;&#10;</p>&#10;<p><code>algorithm</code> is dependent on OpenSSL, examples are <code>&#39;aes192&#39;</code>, etc.  On&#10;recent releases, <code>openssl list-cipher-algorithms</code> will display the&#10;available cipher algorithms.  <code>password</code> is used to derive key and IV,&#10;which must be a <code>&#39;binary&#39;</code> encoded string or a <a href="buffer.html">buffer</a>.&#10;&#10;</p>&#10;<p>It is a <a href="stream.html">stream</a> that is both readable and writable.  The&#10;written data is used to compute the hash.  Once the writable side of&#10;the stream is ended, use the <code>read()</code> method to get the computed hash&#10;digest.  The legacy <code>update</code> and <code>digest</code> methods are also supported.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="algorithm"></param>
                    /// <param name="password"></param>
                    /// </signature>
                    return new this.Cipher();
                }
                this.createCipheriv = function(algorithm, key, iv) {
                    /// <summary><p>Creates and returns a cipher object, with the given algorithm, key and&#10;iv.&#10;&#10;</p>&#10;<p><code>algorithm</code> is the same as the argument to <code>createCipher()</code>.  <code>key</code> is&#10;the raw key used by the algorithm.  <code>iv</code> is an <a href="http://en.wikipedia.org/wiki/Initialization_vector">initialization&#10;vector</a>.&#10;&#10;</p>&#10;<p><code>key</code> and <code>iv</code> must be <code>&#39;binary&#39;</code> encoded strings or&#10;<a href="buffer.html">buffers</a>.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="algorithm"></param>
                    /// <param name="key"></param>
                    /// <param name="iv"></param>
                    /// </signature>
                    return new this.Cipheriv();
                }
                this.createDecipher = function(algorithm, password) {
                    /// <summary><p>Creates and returns a decipher object, with the given algorithm and&#10;key.  This is the mirror of the [createCipher()][] above.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="algorithm"></param>
                    /// <param name="password"></param>
                    /// </signature>
                    return new this.Decipher();
                }
                this.createDecipheriv = function(algorithm, key, iv) {
                    /// <summary><p>Creates and returns a decipher object, with the given algorithm, key&#10;and iv.  This is the mirror of the [createCipheriv()][] above.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="algorithm"></param>
                    /// <param name="key"></param>
                    /// <param name="iv"></param>
                    /// </signature>
                    return new this.Decipheriv();
                }
                this.createSign = function(algorithm) {
                    /// <summary><p>Creates and returns a signing object, with the given algorithm.  On&#10;recent OpenSSL releases, <code>openssl list-public-key-algorithms</code> will&#10;display the available signing algorithms. Examples are <code>&#39;RSA-SHA256&#39;</code>.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="algorithm"></param>
                    /// </signature>
                    return new this.Sign();
                }
                this.createVerify = function(algorithm) {
                    /// <summary><p>Creates and returns a verification object, with the given algorithm.&#10;This is the mirror of the signing object above.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="algorithm"></param>
                    /// </signature>
                    return new this.Verify();
                }
                this.createDiffieHellman = function(prime_length) {
                    /// <summary><p>Creates a Diffie-Hellman key exchange object and generates a prime of&#10;the given bit length. The generator used is <code>2</code>.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="prime_length"></param>
                    /// </signature>
                    return new this.DiffieHellman();
                }
                this.createDiffieHellman = function(prime, encoding) {
                    /// <summary><p>Creates a Diffie-Hellman key exchange object using the supplied prime.&#10;The generator used is <code>2</code>. Encoding can be <code>&#39;binary&#39;</code>, <code>&#39;hex&#39;</code>, or&#10;<code>&#39;base64&#39;</code>.  If no encoding is specified, then a buffer is expected.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="prime"></param>
                    /// <param name="encoding"></param>
                    /// </signature>
                    return new this.DiffieHellman();
                }
                this.getDiffieHellman = function(group_name) {
                    /// <summary><p>Creates a predefined Diffie-Hellman key exchange object.  The&#10;supported groups are: <code>&#39;modp1&#39;</code>, <code>&#39;modp2&#39;</code>, <code>&#39;modp5&#39;</code> (defined in [RFC&#10;2412][]) and <code>&#39;modp14&#39;</code>, <code>&#39;modp15&#39;</code>, <code>&#39;modp16&#39;</code>, <code>&#39;modp17&#39;</code>,&#10;<code>&#39;modp18&#39;</code> (defined in [RFC 3526][]).  The returned object mimics the&#10;interface of objects created by [crypto.createDiffieHellman()][]&#10;above, but will not allow to change the keys (with&#10;[diffieHellman.setPublicKey()][] for example).  The advantage of using&#10;this routine is that the parties don&#39;t have to generate nor exchange&#10;group modulus beforehand, saving both processor and communication&#10;time.&#10;&#10;</p>&#10;<p>Example (obtaining a shared secret):&#10;&#10;</p>&#10;<pre><code>var crypto = require(&#39;crypto&#39;);&#10;var alice = crypto.getDiffieHellman(&#39;modp5&#39;);&#10;var bob = crypto.getDiffieHellman(&#39;modp5&#39;);&#10;&#10;alice.generateKeys();&#10;bob.generateKeys();&#10;&#10;var alice_secret = alice.computeSecret(bob.getPublicKey(), null, &#39;hex&#39;);&#10;var bob_secret = bob.computeSecret(alice.getPublicKey(), null, &#39;hex&#39;);&#10;&#10;/* alice_secret and bob_secret should be the same */&#10;console.log(alice_secret == bob_secret);</code></pre>&#10;</summary>
                    /// <signature>
                    /// <param name="group_name"></param>
                    /// </signature>
                }
                this.pbkdf2 = function(password, salt, iterations, keylen, callback) {
                    /// <summary><p>Asynchronous PBKDF2 applies pseudorandom function HMAC-SHA1 to derive&#10;a key of given length from the given password, salt and iterations.&#10;The callback gets two arguments <code>(err, derivedKey)</code>.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="password"></param>
                    /// <param name="salt"></param>
                    /// <param name="iterations"></param>
                    /// <param name="keylen"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.pbkdf2Sync = function(password, salt, iterations, keylen) {
                    /// <summary><p>Synchronous PBKDF2 function.  Returns derivedKey or throws error.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="password"></param>
                    /// <param name="salt"></param>
                    /// <param name="iterations"></param>
                    /// <param name="keylen"></param>
                    /// </signature>
                }
                this.randomBytes = function(size, callback) {
                    /// <summary><p>Generates cryptographically strong pseudo-random data. Usage:&#10;&#10;</p>&#10;<pre><code>// async&#10;crypto.randomBytes(256, function(ex, buf) {&#10;  if (ex) throw ex;&#10;  console.log(&#39;Have %d bytes of random data: %s&#39;, buf.length, buf);&#10;});&#10;&#10;// sync&#10;try {&#10;  var buf = crypto.randomBytes(256);&#10;  console.log(&#39;Have %d bytes of random data: %s&#39;, buf.length, buf);&#10;} catch (ex) {&#10;  // handle error&#10;}</code></pre>&#10;</summary>
                    /// <signature>
                    /// <param name="size"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.pseudoRandomBytes = function(size, callback) {
                    /// <summary><p>Generates <em>non</em>-cryptographically strong pseudo-random data. The data&#10;returned will be unique if it is sufficiently long, but is not&#10;necessarily unpredictable. For this reason, the output of this&#10;function should never be used where unpredictability is important,&#10;such as in the generation of encryption keys.&#10;&#10;</p>&#10;<p>Usage is otherwise identical to <code>crypto.randomBytes</code>.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="size"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                function _Hash() {
                    this.update = function(data, input_encoding) {
                        /// <summary><p>Updates the hash content with the given <code>data</code>, the encoding of which&#10;is given in <code>input_encoding</code> and can be <code>&#39;utf8&#39;</code>, <code>&#39;ascii&#39;</code> or&#10;<code>&#39;binary&#39;</code>.  If no encoding is provided, then a buffer is expected.&#10;&#10;</p>&#10;<p>This can be called many times with new data as it is streamed.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="data"></param>
                        /// <param name="input_encoding"></param>
                        /// </signature>
                    }
                    this.digest = function(encoding) {
                        /// <summary><p>Calculates the digest of all of the passed data to be hashed.  The&#10;<code>encoding</code> can be <code>&#39;hex&#39;</code>, <code>&#39;binary&#39;</code> or <code>&#39;base64&#39;</code>.  If no encoding&#10;is provided, then a buffer is returned.&#10;&#10;</p>&#10;<p>Note: <code>hash</code> object can not be used after <code>digest()</code> method has been&#10;called.&#10;&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="encoding"></param>
                        /// </signature>
                    }
                }

                this.Hash = function() {
                    return new _Hash();
                }
                function _Hmac() {
                    this.update = function(data) {
                        /// <summary><p>Update the hmac content with the given <code>data</code>.  This can be called&#10;many times with new data as it is streamed.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="data"></param>
                        /// </signature>
                    }
                    this.digest = function(encoding) {
                        /// <summary><p>Calculates the digest of all of the passed data to the hmac.  The&#10;<code>encoding</code> can be <code>&#39;hex&#39;</code>, <code>&#39;binary&#39;</code> or <code>&#39;base64&#39;</code>.  If no encoding&#10;is provided, then a buffer is returned.&#10;&#10;</p>&#10;<p>Note: <code>hmac</code> object can not be used after <code>digest()</code> method has been&#10;called.&#10;&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="encoding"></param>
                        /// </signature>
                    }
                }

                this.Hmac = function() {
                    return new _Hmac();
                }
                function _Cipher() {
                    this.update = function(data, input_encoding, output_encoding) {
                        /// <summary><p>Updates the cipher with <code>data</code>, the encoding of which is given in&#10;<code>input_encoding</code> and can be <code>&#39;utf8&#39;</code>, <code>&#39;ascii&#39;</code> or <code>&#39;binary&#39;</code>.  If no&#10;encoding is provided, then a buffer is expected.&#10;&#10;</p>&#10;<p>The <code>output_encoding</code> specifies the output format of the enciphered&#10;data, and can be <code>&#39;binary&#39;</code>, <code>&#39;base64&#39;</code> or <code>&#39;hex&#39;</code>.  If no encoding is&#10;provided, then a buffer is returned.&#10;&#10;</p>&#10;<p>Returns the enciphered contents, and can be called many times with new&#10;data as it is streamed.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="data"></param>
                        /// <param name="input_encoding"></param>
                        /// <param name="output_encoding"></param>
                        /// </signature>
                    }
                    this.final = function(output_encoding) {
                        /// <summary><p>Returns any remaining enciphered contents, with <code>output_encoding</code>&#10;being one of: <code>&#39;binary&#39;</code>, <code>&#39;base64&#39;</code> or <code>&#39;hex&#39;</code>.  If no encoding is&#10;provided, then a buffer is returned.&#10;&#10;</p>&#10;<p>Note: <code>cipher</code> object can not be used after <code>final()</code> method has been&#10;called.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="output_encoding"></param>
                        /// </signature>
                    }
                    this.setAutoPadding = function(auto_padding) {
                        /// <summary><p>You can disable automatic padding of the input data to block size. If&#10;<code>auto_padding</code> is false, the length of the entire input data must be a&#10;multiple of the cipher&#39;s block size or <code>final</code> will fail.  Useful for&#10;non-standard padding, e.g. using <code>0x0</code> instead of PKCS padding. You&#10;must call this before <code>cipher.final</code>.&#10;&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="auto_padding"></param>
                        /// </signature>
                    }
                }

                this.Cipher = function() {
                    return new _Cipher();
                }
                function _Decipher() {
                    this.update = function(data, input_encoding, output_encoding) {
                        /// <summary><p>Updates the decipher with <code>data</code>, which is encoded in <code>&#39;binary&#39;</code>,&#10;<code>&#39;base64&#39;</code> or <code>&#39;hex&#39;</code>.  If no encoding is provided, then a buffer is&#10;expected.&#10;&#10;</p>&#10;<p>The <code>output_decoding</code> specifies in what format to return the&#10;deciphered plaintext: <code>&#39;binary&#39;</code>, <code>&#39;ascii&#39;</code> or <code>&#39;utf8&#39;</code>.  If no&#10;encoding is provided, then a buffer is returned.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="data"></param>
                        /// <param name="input_encoding"></param>
                        /// <param name="output_encoding"></param>
                        /// </signature>
                    }
                    this.final = function(output_encoding) {
                        /// <summary><p>Returns any remaining plaintext which is deciphered, with&#10;<code>output_encoding</code> being one of: <code>&#39;binary&#39;</code>, <code>&#39;ascii&#39;</code> or <code>&#39;utf8&#39;</code>.  If&#10;no encoding is provided, then a buffer is returned.&#10;&#10;</p>&#10;<p>Note: <code>decipher</code> object can not be used after <code>final()</code> method has been&#10;called.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="output_encoding"></param>
                        /// </signature>
                    }
                    this.setAutoPadding = function(auto_padding) {
                        /// <summary><p>You can disable auto padding if the data has been encrypted without&#10;standard block padding to prevent <code>decipher.final</code> from checking and&#10;removing it. Can only work if the input data&#39;s length is a multiple of&#10;the ciphers block size. You must call this before streaming data to&#10;<code>decipher.update</code>.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="auto_padding"></param>
                        /// </signature>
                    }
                }

                this.Decipher = function() {
                    return new _Decipher();
                }
                function _Sign() {
                    this.update = function(data) {
                        /// <summary><p>Updates the sign object with data.  This can be called many times&#10;with new data as it is streamed.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="data"></param>
                        /// </signature>
                    }
                    this.sign = function(private_key, output_format) {
                        /// <summary><p>Calculates the signature on all the updated data passed through the&#10;sign.  <code>private_key</code> is a string containing the PEM encoded private&#10;key for signing.&#10;&#10;</p>&#10;<p>Returns the signature in <code>output_format</code> which can be <code>&#39;binary&#39;</code>,&#10;<code>&#39;hex&#39;</code> or <code>&#39;base64&#39;</code>. If no encoding is provided, then a buffer is&#10;returned.&#10;&#10;</p>&#10;<p>Note: <code>sign</code> object can not be used after <code>sign()</code> method has been&#10;called.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="private_key"></param>
                        /// <param name="output_format"></param>
                        /// </signature>
                    }
                }

                this.Sign = function() {
                    return new _Sign();
                }
                function _Verify() {
                    this.update = function(data) {
                        /// <summary><p>Updates the verifier object with data.  This can be called many times&#10;with new data as it is streamed.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="data"></param>
                        /// </signature>
                    }
                    this.verify = function(object, signature, signature_format) {
                        /// <summary><p>Verifies the signed data by using the <code>object</code> and <code>signature</code>.&#10;<code>object</code> is  a string containing a PEM encoded object, which can be&#10;one of RSA public key, DSA public key, or X.509 certificate.&#10;<code>signature</code> is the previously calculated signature for the data, in&#10;the <code>signature_format</code> which can be <code>&#39;binary&#39;</code>, <code>&#39;hex&#39;</code> or <code>&#39;base64&#39;</code>.&#10;If no encoding is specified, then a buffer is expected.&#10;&#10;</p>&#10;<p>Returns true or false depending on the validity of the signature for&#10;the data and public key.&#10;&#10;</p>&#10;<p>Note: <code>verifier</code> object can not be used after <code>verify()</code> method has been&#10;called.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="object"></param>
                        /// <param name="signature"></param>
                        /// <param name="signature_format"></param>
                        /// </signature>
                    }
                }

                this.Verify = function() {
                    return new _Verify();
                }
                function _DiffieHellman() {
                    this.generateKeys = function(encoding) {
                        /// <summary><p>Generates private and public Diffie-Hellman key values, and returns&#10;the public key in the specified encoding. This key should be&#10;transferred to the other party. Encoding can be <code>&#39;binary&#39;</code>, <code>&#39;hex&#39;</code>,&#10;or <code>&#39;base64&#39;</code>.  If no encoding is provided, then a buffer is returned.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="encoding"></param>
                        /// </signature>
                    }
                    this.computeSecret = function(other_public_key, input_encoding, output_encoding) {
                        /// <summary><p>Computes the shared secret using <code>other_public_key</code> as the other&#10;party&#39;s public key and returns the computed shared secret. Supplied&#10;key is interpreted using specified <code>input_encoding</code>, and secret is&#10;encoded using specified <code>output_encoding</code>. Encodings can be&#10;<code>&#39;binary&#39;</code>, <code>&#39;hex&#39;</code>, or <code>&#39;base64&#39;</code>. If the input encoding is not&#10;provided, then a buffer is expected.&#10;&#10;</p>&#10;<p>If no output encoding is given, then a buffer is returned.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="other_public_key"></param>
                        /// <param name="input_encoding"></param>
                        /// <param name="output_encoding"></param>
                        /// </signature>
                    }
                    this.getPrime = function(encoding) {
                        /// <summary><p>Returns the Diffie-Hellman prime in the specified encoding, which can&#10;be <code>&#39;binary&#39;</code>, <code>&#39;hex&#39;</code>, or <code>&#39;base64&#39;</code>. If no encoding is provided,&#10;then a buffer is returned.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="encoding"></param>
                        /// </signature>
                    }
                    this.getGenerator = function(encoding) {
                        /// <summary><p>Returns the Diffie-Hellman prime in the specified encoding, which can&#10;be <code>&#39;binary&#39;</code>, <code>&#39;hex&#39;</code>, or <code>&#39;base64&#39;</code>. If no encoding is provided,&#10;then a buffer is returned.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="encoding"></param>
                        /// </signature>
                    }
                    this.getPublicKey = function(encoding) {
                        /// <summary><p>Returns the Diffie-Hellman public key in the specified encoding, which&#10;can be <code>&#39;binary&#39;</code>, <code>&#39;hex&#39;</code>, or <code>&#39;base64&#39;</code>. If no encoding is provided,&#10;then a buffer is returned.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="encoding"></param>
                        /// </signature>
                    }
                    this.getPrivateKey = function(encoding) {
                        /// <summary><p>Returns the Diffie-Hellman private key in the specified encoding,&#10;which can be <code>&#39;binary&#39;</code>, <code>&#39;hex&#39;</code>, or <code>&#39;base64&#39;</code>. If no encoding is&#10;provided, then a buffer is returned.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="encoding"></param>
                        /// </signature>
                    }
                    this.setPublicKey = function(public_key, encoding) {
                        /// <summary><p>Sets the Diffie-Hellman public key. Key encoding can be <code>&#39;binary&#39;</code>,&#10;<code>&#39;hex&#39;</code> or <code>&#39;base64&#39;</code>. If no encoding is provided, then a buffer is&#10;expected.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="public_key"></param>
                        /// <param name="encoding"></param>
                        /// </signature>
                    }
                    this.setPrivateKey = function(private_key, encoding) {
                        /// <summary><p>Sets the Diffie-Hellman private key. Key encoding can be <code>&#39;binary&#39;</code>,&#10;<code>&#39;hex&#39;</code> or <code>&#39;base64&#39;</code>. If no encoding is provided, then a buffer is&#10;expected.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="private_key"></param>
                        /// <param name="encoding"></param>
                        /// </signature>
                    }
                }

                this.DiffieHellman = function() {
                    return new _DiffieHellman();
                }
                /// <field name='DEFAULT_ENCODING'><p>The default encoding to use for functions that can take either strings&#10;or buffers.  The default value is <code>&#39;buffer&#39;</code>, which makes it default&#10;to using Buffer objects.  This is here to make the crypto module more&#10;easily compatible with legacy programs that expected <code>&#39;binary&#39;</code> to be&#10;the default encoding.&#10;&#10;</p>&#10;<p>Note that new programs will probably expect buffers, so only use this&#10;as a temporary measure.&#10;&#10;</p>&#10;</field>
                this.DEFAULT_ENCODING = undefined;
            };
            case "tls": return new     function tls() {
                /// <summary><p>Use <code>require(&#39;tls&#39;)</code> to access this module.&#10;&#10;</p>&#10;<p>The <code>tls</code> module uses OpenSSL to provide Transport Layer Security and/or&#10;Secure Socket Layer: encrypted stream communication.&#10;&#10;</p>&#10;<p>TLS/SSL is a public/private key infrastructure. Each client and each&#10;server must have a private key. A private key is created like this&#10;&#10;</p>&#10;<pre><code>openssl genrsa -out ryans-key.pem 1024</code></pre>&#10;<p>All severs and some clients need to have a certificate. Certificates are public&#10;keys signed by a Certificate Authority or self-signed. The first step to&#10;getting a certificate is to create a &quot;Certificate Signing Request&quot; (CSR)&#10;file. This is done with:&#10;&#10;</p>&#10;<pre><code>openssl req -new -key ryans-key.pem -out ryans-csr.pem</code></pre>&#10;<p>To create a self-signed certificate with the CSR, do this:&#10;&#10;</p>&#10;<pre><code>openssl x509 -req -in ryans-csr.pem -signkey ryans-key.pem -out ryans-cert.pem</code></pre>&#10;<p>Alternatively you can send the CSR to a Certificate Authority for signing.&#10;&#10;</p>&#10;<p>(TODO: docs on creating a CA, for now interested users should just look at&#10;<code>test/fixtures/keys/Makefile</code> in the Node source code)&#10;&#10;</p>&#10;<p>To create .pfx or .p12, do this:&#10;&#10;</p>&#10;<pre><code>openssl pkcs12 -export -in agent5-cert.pem -inkey agent5-key.pem \&#10;    -certfile ca-cert.pem -out agent5.pfx</code></pre>&#10;<ul>&#10;<li><code>in</code>:  certificate</li>&#10;<li><code>inkey</code>: private key</li>&#10;<li><code>certfile</code>: all CA certs concatenated in one file like&#10;<code>cat ca1-cert.pem ca2-cert.pem &gt; ca-cert.pem</code></li>&#10;</ul>&#10;</summary>
                this.getCiphers = function() {
                    /// <summary><p>Returns an array with the names of the supported SSL ciphers.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>var ciphers = tls.getCiphers();&#10;console.log(ciphers); // [&#39;AES128-SHA&#39;, &#39;AES256-SHA&#39;, ...]</code></pre>&#10;</summary>
                }
                this.createServer = function(options, secureConnectionListener) {
                    /// <summary><p>Creates a new [tls.Server][].  The <code>connectionListener</code> argument is&#10;automatically set as a listener for the [secureConnection][] event.  The&#10;<code>options</code> object has these possibilities:&#10;&#10;</p>&#10;<ul>&#10;<li><p><code>pfx</code>: A string or <code>Buffer</code> containing the private key, certificate and&#10;CA certs of the server in PFX or PKCS12 format. (Mutually exclusive with&#10;the <code>key</code>, <code>cert</code> and <code>ca</code> options.)</p>&#10;</li>&#10;<li><p><code>key</code>: A string or <code>Buffer</code> containing the private key of the server in&#10;PEM format. (Required)</p>&#10;</li>&#10;<li><p><code>passphrase</code>: A string of passphrase for the private key or pfx.</p>&#10;</li>&#10;<li><p><code>cert</code>: A string or <code>Buffer</code> containing the certificate key of the server in&#10;PEM format. (Required)</p>&#10;</li>&#10;<li><p><code>ca</code>: An array of strings or <code>Buffer</code>s of trusted certificates in PEM&#10;format. If this is omitted several well known &quot;root&quot; CAs will be used,&#10;like VeriSign. These are used to authorize connections.</p>&#10;</li>&#10;<li><p><code>crl</code> : Either a string or list of strings of PEM encoded CRLs (Certificate&#10;Revocation List)</p>&#10;</li>&#10;<li><p><code>ciphers</code>: A string describing the ciphers to use or exclude.</p>&#10;<p>To mitigate [BEAST attacks] it is recommended that you use this option in&#10;conjunction with the <code>honorCipherOrder</code> option described below to&#10;prioritize the non-CBC cipher.</p>&#10;<p>Defaults to <code>AES128-GCM-SHA256:RC4:HIGH:!MD5:!aNULL:!EDH</code>.&#10;Consult the [OpenSSL cipher list format documentation] for details on the&#10;format. ECDH (Elliptic Curve Diffie-Hellman) ciphers are not yet supported.</p>&#10;</li>&#10;</ul>&#10;<pre><code>`AES128-GCM-SHA256` is used when node.js is linked against OpenSSL 1.0.1&#10;or newer and the client speaks TLS 1.2, RC4 is used as a secure fallback.&#10;&#10;**NOTE**: Previous revisions of this section suggested `AES256-SHA` as an&#10;acceptable cipher. Unfortunately, `AES256-SHA` is a CBC cipher and therefore&#10;susceptible to BEAST attacks. Do *not* use it.</code></pre>&#10;<ul>&#10;<li><p><code>handshakeTimeout</code>: Abort the connection if the SSL/TLS handshake does not&#10;finish in this many milliseconds. The default is 120 seconds.</p>&#10;<p>A <code>&#39;clientError&#39;</code> is emitted on the <code>tls.Server</code> object whenever a handshake&#10;times out.</p>&#10;</li>&#10;<li><p><code>honorCipherOrder</code> : When choosing a cipher, use the server&#39;s preferences&#10;instead of the client preferences.</p>&#10;<p>Note that if SSLv2 is used, the server will send its list of preferences&#10;to the client, and the client chooses the cipher.</p>&#10;<p>Although, this option is disabled by default, it is <em>recommended</em> that you&#10;use this option in conjunction with the <code>ciphers</code> option to mitigate&#10;BEAST attacks.</p>&#10;</li>&#10;<li><p><code>requestCert</code>: If <code>true</code> the server will request a certificate from&#10;clients that connect and attempt to verify that certificate. Default:&#10;<code>false</code>.</p>&#10;</li>&#10;<li><p><code>rejectUnauthorized</code>: If <code>true</code> the server will reject any connection&#10;which is not authorized with the list of supplied CAs. This option only&#10;has an effect if <code>requestCert</code> is <code>true</code>. Default: <code>false</code>.</p>&#10;</li>&#10;<li><p><code>NPNProtocols</code>: An array or <code>Buffer</code> of possible NPN protocols. (Protocols&#10;should be ordered by their priority).</p>&#10;</li>&#10;<li><p><code>SNICallback</code>: A function that will be called if client supports SNI TLS&#10;extension. Only one argument will be passed to it: <code>servername</code>. And&#10;<code>SNICallback</code> should return SecureContext instance.&#10;(You can use <code>crypto.createCredentials(...).context</code> to get proper&#10;SecureContext). If <code>SNICallback</code> wasn&#39;t provided - default callback with&#10;high-level API will be used (see below).</p>&#10;</li>&#10;<li><p><code>sessionIdContext</code>: A string containing a opaque identifier for session&#10;resumption. If <code>requestCert</code> is <code>true</code>, the default is MD5 hash value&#10;generated from command-line. Otherwise, the default is not provided.</p>&#10;</li>&#10;<li><p><code>secureProtocol</code>: The SSL method to use, e.g. <code>SSLv3_method</code> to force&#10;SSL version 3. The possible values depend on your installation of&#10;OpenSSL and are defined in the constant [SSL_METHODS][].</p>&#10;</li>&#10;</ul>&#10;<p>Here is a simple example echo server:&#10;&#10;</p>&#10;<pre><code>var tls = require(&#39;tls&#39;);&#10;var fs = require(&#39;fs&#39;);&#10;&#10;var options = {&#10;  key: fs.readFileSync(&#39;server-key.pem&#39;),&#10;  cert: fs.readFileSync(&#39;server-cert.pem&#39;),&#10;&#10;  // This is necessary only if using the client certificate authentication.&#10;  requestCert: true,&#10;&#10;  // This is necessary only if the client uses the self-signed certificate.&#10;  ca: [ fs.readFileSync(&#39;client-cert.pem&#39;) ]&#10;};&#10;&#10;var server = tls.createServer(options, function(cleartextStream) {&#10;  console.log(&#39;server connected&#39;,&#10;              cleartextStream.authorized ? &#39;authorized&#39; : &#39;unauthorized&#39;);&#10;  cleartextStream.write(&quot;welcome!\n&quot;);&#10;  cleartextStream.setEncoding(&#39;utf8&#39;);&#10;  cleartextStream.pipe(cleartextStream);&#10;});&#10;server.listen(8000, function() {&#10;  console.log(&#39;server bound&#39;);&#10;});</code></pre>&#10;<p>Or&#10;&#10;</p>&#10;<pre><code>var tls = require(&#39;tls&#39;);&#10;var fs = require(&#39;fs&#39;);&#10;&#10;var options = {&#10;  pfx: fs.readFileSync(&#39;server.pfx&#39;),&#10;&#10;  // This is necessary only if using the client certificate authentication.&#10;  requestCert: true,&#10;&#10;};&#10;&#10;var server = tls.createServer(options, function(cleartextStream) {&#10;  console.log(&#39;server connected&#39;,&#10;              cleartextStream.authorized ? &#39;authorized&#39; : &#39;unauthorized&#39;);&#10;  cleartextStream.write(&quot;welcome!\n&quot;);&#10;  cleartextStream.setEncoding(&#39;utf8&#39;);&#10;  cleartextStream.pipe(cleartextStream);&#10;});&#10;server.listen(8000, function() {&#10;  console.log(&#39;server bound&#39;);&#10;});</code></pre>&#10;<p>You can test this server by connecting to it with <code>openssl s_client</code>:&#10;&#10;&#10;</p>&#10;<pre><code>openssl s_client -connect 127.0.0.1:8000</code></pre>&#10;</summary>
                    /// <signature>
                    /// <param name="options"></param>
                    /// <param name="secureConnectionListener"></param>
                    /// </signature>
                    return new this.Server();
                }
                this.connect = function(port, host, options, callback) {
                    /// <summary><p>Creates a new client connection to the given <code>port</code> and <code>host</code> (old API) or&#10;<code>options.port</code> and <code>options.host</code>. (If <code>host</code> is omitted, it defaults to&#10;<code>localhost</code>.) <code>options</code> should be an object which specifies:&#10;&#10;</p>&#10;<ul>&#10;<li><p><code>host</code>: Host the client should connect to</p>&#10;</li>&#10;<li><p><code>port</code>: Port the client should connect to</p>&#10;</li>&#10;<li><p><code>socket</code>: Establish secure connection on a given socket rather than&#10;creating a new socket. If this option is specified, <code>host</code> and <code>port</code>&#10;are ignored.</p>&#10;</li>&#10;<li><p><code>pfx</code>: A string or <code>Buffer</code> containing the private key, certificate and&#10;CA certs of the server in PFX or PKCS12 format.</p>&#10;</li>&#10;<li><p><code>key</code>: A string or <code>Buffer</code> containing the private key of the client in&#10;PEM format.</p>&#10;</li>&#10;<li><p><code>passphrase</code>: A string of passphrase for the private key or pfx.</p>&#10;</li>&#10;<li><p><code>cert</code>: A string or <code>Buffer</code> containing the certificate key of the client in&#10;PEM format.</p>&#10;</li>&#10;<li><p><code>ca</code>: An array of strings or <code>Buffer</code>s of trusted certificates in PEM&#10;format. If this is omitted several well known &quot;root&quot; CAs will be used,&#10;like VeriSign. These are used to authorize connections.</p>&#10;</li>&#10;<li><p><code>rejectUnauthorized</code>: If <code>true</code>, the server certificate is verified against&#10;the list of supplied CAs. An <code>&#39;error&#39;</code> event is emitted if verification&#10;fails. Default: <code>true</code>.</p>&#10;</li>&#10;<li><p><code>NPNProtocols</code>: An array of strings or <code>Buffer</code>s containing supported NPN&#10;protocols. <code>Buffer</code>s should have following format: <code>0x05hello0x05world</code>,&#10;where first byte is next protocol name&#39;s length. (Passing array should&#10;usually be much simpler: <code>[&#39;hello&#39;, &#39;world&#39;]</code>.)</p>&#10;</li>&#10;<li><p><code>servername</code>: Servername for SNI (Server Name Indication) TLS extension.</p>&#10;</li>&#10;<li><p><code>secureProtocol</code>: The SSL method to use, e.g. <code>SSLv3_method</code> to force&#10;SSL version 3. The possible values depend on your installation of&#10;OpenSSL and are defined in the constant [SSL_METHODS][].</p>&#10;</li>&#10;</ul>&#10;<p>The <code>callback</code> parameter will be added as a listener for the&#10;[&#39;secureConnect&#39;][] event.&#10;&#10;</p>&#10;<p><code>tls.connect()</code> returns a [CleartextStream][] object.&#10;&#10;</p>&#10;<p>Here is an example of a client of echo server as described previously:&#10;&#10;</p>&#10;<pre><code>var tls = require(&#39;tls&#39;);&#10;var fs = require(&#39;fs&#39;);&#10;&#10;var options = {&#10;  // These are necessary only if using the client certificate authentication&#10;  key: fs.readFileSync(&#39;client-key.pem&#39;),&#10;  cert: fs.readFileSync(&#39;client-cert.pem&#39;),&#10;&#10;  // This is necessary only if the server uses the self-signed certificate&#10;  ca: [ fs.readFileSync(&#39;server-cert.pem&#39;) ]&#10;};&#10;&#10;var cleartextStream = tls.connect(8000, options, function() {&#10;  console.log(&#39;client connected&#39;,&#10;              cleartextStream.authorized ? &#39;authorized&#39; : &#39;unauthorized&#39;);&#10;  process.stdin.pipe(cleartextStream);&#10;  process.stdin.resume();&#10;});&#10;cleartextStream.setEncoding(&#39;utf8&#39;);&#10;cleartextStream.on(&#39;data&#39;, function(data) {&#10;  console.log(data);&#10;});&#10;cleartextStream.on(&#39;end&#39;, function() {&#10;  server.close();&#10;});</code></pre>&#10;<p>Or&#10;&#10;</p>&#10;<pre><code>var tls = require(&#39;tls&#39;);&#10;var fs = require(&#39;fs&#39;);&#10;&#10;var options = {&#10;  pfx: fs.readFileSync(&#39;client.pfx&#39;)&#10;};&#10;&#10;var cleartextStream = tls.connect(8000, options, function() {&#10;  console.log(&#39;client connected&#39;,&#10;              cleartextStream.authorized ? &#39;authorized&#39; : &#39;unauthorized&#39;);&#10;  process.stdin.pipe(cleartextStream);&#10;  process.stdin.resume();&#10;});&#10;cleartextStream.setEncoding(&#39;utf8&#39;);&#10;cleartextStream.on(&#39;data&#39;, function(data) {&#10;  console.log(data);&#10;});&#10;cleartextStream.on(&#39;end&#39;, function() {&#10;  server.close();&#10;});</code></pre>&#10;</summary>
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
                    /// <summary><p>Creates a new client connection to the given <code>port</code> and <code>host</code> (old API) or&#10;<code>options.port</code> and <code>options.host</code>. (If <code>host</code> is omitted, it defaults to&#10;<code>localhost</code>.) <code>options</code> should be an object which specifies:&#10;&#10;</p>&#10;<ul>&#10;<li><p><code>host</code>: Host the client should connect to</p>&#10;</li>&#10;<li><p><code>port</code>: Port the client should connect to</p>&#10;</li>&#10;<li><p><code>socket</code>: Establish secure connection on a given socket rather than&#10;creating a new socket. If this option is specified, <code>host</code> and <code>port</code>&#10;are ignored.</p>&#10;</li>&#10;<li><p><code>pfx</code>: A string or <code>Buffer</code> containing the private key, certificate and&#10;CA certs of the server in PFX or PKCS12 format.</p>&#10;</li>&#10;<li><p><code>key</code>: A string or <code>Buffer</code> containing the private key of the client in&#10;PEM format.</p>&#10;</li>&#10;<li><p><code>passphrase</code>: A string of passphrase for the private key or pfx.</p>&#10;</li>&#10;<li><p><code>cert</code>: A string or <code>Buffer</code> containing the certificate key of the client in&#10;PEM format.</p>&#10;</li>&#10;<li><p><code>ca</code>: An array of strings or <code>Buffer</code>s of trusted certificates in PEM&#10;format. If this is omitted several well known &quot;root&quot; CAs will be used,&#10;like VeriSign. These are used to authorize connections.</p>&#10;</li>&#10;<li><p><code>rejectUnauthorized</code>: If <code>true</code>, the server certificate is verified against&#10;the list of supplied CAs. An <code>&#39;error&#39;</code> event is emitted if verification&#10;fails. Default: <code>true</code>.</p>&#10;</li>&#10;<li><p><code>NPNProtocols</code>: An array of strings or <code>Buffer</code>s containing supported NPN&#10;protocols. <code>Buffer</code>s should have following format: <code>0x05hello0x05world</code>,&#10;where first byte is next protocol name&#39;s length. (Passing array should&#10;usually be much simpler: <code>[&#39;hello&#39;, &#39;world&#39;]</code>.)</p>&#10;</li>&#10;<li><p><code>servername</code>: Servername for SNI (Server Name Indication) TLS extension.</p>&#10;</li>&#10;<li><p><code>secureProtocol</code>: The SSL method to use, e.g. <code>SSLv3_method</code> to force&#10;SSL version 3. The possible values depend on your installation of&#10;OpenSSL and are defined in the constant [SSL_METHODS][].</p>&#10;</li>&#10;</ul>&#10;<p>The <code>callback</code> parameter will be added as a listener for the&#10;[&#39;secureConnect&#39;][] event.&#10;&#10;</p>&#10;<p><code>tls.connect()</code> returns a [CleartextStream][] object.&#10;&#10;</p>&#10;<p>Here is an example of a client of echo server as described previously:&#10;&#10;</p>&#10;<pre><code>var tls = require(&#39;tls&#39;);&#10;var fs = require(&#39;fs&#39;);&#10;&#10;var options = {&#10;  // These are necessary only if using the client certificate authentication&#10;  key: fs.readFileSync(&#39;client-key.pem&#39;),&#10;  cert: fs.readFileSync(&#39;client-cert.pem&#39;),&#10;&#10;  // This is necessary only if the server uses the self-signed certificate&#10;  ca: [ fs.readFileSync(&#39;server-cert.pem&#39;) ]&#10;};&#10;&#10;var cleartextStream = tls.connect(8000, options, function() {&#10;  console.log(&#39;client connected&#39;,&#10;              cleartextStream.authorized ? &#39;authorized&#39; : &#39;unauthorized&#39;);&#10;  process.stdin.pipe(cleartextStream);&#10;  process.stdin.resume();&#10;});&#10;cleartextStream.setEncoding(&#39;utf8&#39;);&#10;cleartextStream.on(&#39;data&#39;, function(data) {&#10;  console.log(data);&#10;});&#10;cleartextStream.on(&#39;end&#39;, function() {&#10;  server.close();&#10;});</code></pre>&#10;<p>Or&#10;&#10;</p>&#10;<pre><code>var tls = require(&#39;tls&#39;);&#10;var fs = require(&#39;fs&#39;);&#10;&#10;var options = {&#10;  pfx: fs.readFileSync(&#39;client.pfx&#39;)&#10;};&#10;&#10;var cleartextStream = tls.connect(8000, options, function() {&#10;  console.log(&#39;client connected&#39;,&#10;              cleartextStream.authorized ? &#39;authorized&#39; : &#39;unauthorized&#39;);&#10;  process.stdin.pipe(cleartextStream);&#10;  process.stdin.resume();&#10;});&#10;cleartextStream.setEncoding(&#39;utf8&#39;);&#10;cleartextStream.on(&#39;data&#39;, function(data) {&#10;  console.log(data);&#10;});&#10;cleartextStream.on(&#39;end&#39;, function() {&#10;  server.close();&#10;});</code></pre>&#10;</summary>
                    /// <signature>
                    /// <param name="port"></param>
                    /// <param name="host"></param>
                    /// <param name="options"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.createSecurePair = function(credentials, isServer, requestCert, rejectUnauthorized) {
                    /// <summary><p>Creates a new secure pair object with two streams, one of which reads/writes&#10;encrypted data, and one reads/writes cleartext data.&#10;Generally the encrypted one is piped to/from an incoming encrypted data stream,&#10;and the cleartext one is used as a replacement for the initial encrypted stream.&#10;&#10;</p>&#10;<ul>&#10;<li><p><code>credentials</code>: A credentials object from crypto.createCredentials( ... )</p>&#10;</li>&#10;<li><p><code>isServer</code>: A boolean indicating whether this tls connection should be&#10;opened as a server or a client.</p>&#10;</li>&#10;<li><p><code>requestCert</code>: A boolean indicating whether a server should request a&#10;certificate from a connecting client. Only applies to server connections.</p>&#10;</li>&#10;<li><p><code>rejectUnauthorized</code>: A boolean indicating whether a server should&#10;automatically reject clients with invalid certificates. Only applies to&#10;servers with <code>requestCert</code> enabled.</p>&#10;</li>&#10;</ul>&#10;<p><code>tls.createSecurePair()</code> returns a SecurePair object with [cleartext][] and&#10;<code>encrypted</code> stream properties.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="credentials"></param>
                    /// <param name="isServer"></param>
                    /// <param name="requestCert"></param>
                    /// <param name="rejectUnauthorized"></param>
                    /// </signature>
                    return new this.SecurePair();
                }
                function _SecurePair() {
                    this.addListener = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// secure: The event is emitted from the SecurePair once the pair has successfully ...&#10;
                        /// </summary>

                    }
                    this.once = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// secure: The event is emitted from the SecurePair once the pair has successfully ...&#10;
                        /// </summary>

                    }
                    this.removeListener = function(event, listener) {
                        /// <summary>Supported Events: secure</summary>
                    }
                    this.removeAllListeners = function(event) {
                        /// <summary>Supported Events: secure</summary>
                    }
                    this.setMaxListeners = function(n) { }
                    this.listeners = function(event) {
                        /// <summary>Supported Events: secure</summary>
                    }
                    this.emit = function(event, arguments) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// secure: The event is emitted from the SecurePair once the pair has successfully ...&#10;
                        /// </summary>

                    }
                    this.on = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// secure: The event is emitted from the SecurePair once the pair has successfully ...&#10;
                        /// </summary>

                    }
                }

                this.SecurePair = function() {
                    return new _SecurePair();
                }
                function _Server() {
                    this.listen = function(port, host, callback) {
                        /// <summary><p>Begin accepting connections on the specified <code>port</code> and <code>host</code>.  If the&#10;<code>host</code> is omitted, the server will accept connections directed to any&#10;IPv4 address (<code>INADDR_ANY</code>).&#10;&#10;</p>&#10;<p>This function is asynchronous. The last parameter <code>callback</code> will be called&#10;when the server has been bound.&#10;&#10;</p>&#10;<p>See <code>net.Server</code> for more information.&#10;&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="port"></param>
                        /// <param name="host"></param>
                        /// <param name="callback"></param>
                        /// </signature>
                    }
                    this.close = function() {
                        /// <summary><p>Stops the server from accepting new connections. This function is&#10;asynchronous, the server is finally closed when the server emits a <code>&#39;close&#39;</code>&#10;event.&#10;&#10;</p>&#10;</summary>
                    }
                    this.address = function() {
                        /// <summary><p>Returns the bound address, the address family name and port of the&#10;server as reported by the operating system.  See [net.Server.address()][] for&#10;more information.&#10;&#10;</p>&#10;</summary>
                    }
                    this.addContext = function(hostname, credentials) {
                        /// <summary><p>Add secure context that will be used if client request&#39;s SNI hostname is&#10;matching passed <code>hostname</code> (wildcards can be used). <code>credentials</code> can contain&#10;<code>key</code>, <code>cert</code> and <code>ca</code>.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="hostname"></param>
                        /// <param name="credentials"></param>
                        /// </signature>
                    }
                    this.addListener = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// secureConnection: <code>function (cleartextStream) {}</code> ...&#10;
                        /// clientError: <code>function (exception, securePair) { }</code> ...&#10;
                        /// newSession: <code>function (sessionId, sessionData) { }</code> ...&#10;
                        /// resumeSession: <code>function (sessionId, callback) { }</code> ...&#10;
                        /// </summary>

                    }
                    this.once = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// secureConnection: <code>function (cleartextStream) {}</code> ...&#10;
                        /// clientError: <code>function (exception, securePair) { }</code> ...&#10;
                        /// newSession: <code>function (sessionId, sessionData) { }</code> ...&#10;
                        /// resumeSession: <code>function (sessionId, callback) { }</code> ...&#10;
                        /// </summary>

                    }
                    this.removeListener = function(event, listener) {
                        /// <summary>Supported Events: secureConnection, clientError, newSession, resumeSession</summary>
                    }
                    this.removeAllListeners = function(event) {
                        /// <summary>Supported Events: secureConnection, clientError, newSession, resumeSession</summary>
                    }
                    this.setMaxListeners = function(n) { }
                    this.listeners = function(event) {
                        /// <summary>Supported Events: secureConnection, clientError, newSession, resumeSession</summary>
                    }
                    this.emit = function(event, arguments) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// secureConnection: <code>function (cleartextStream) {}</code> ...&#10;
                        /// clientError: <code>function (exception, securePair) { }</code> ...&#10;
                        /// newSession: <code>function (sessionId, sessionData) { }</code> ...&#10;
                        /// resumeSession: <code>function (sessionId, callback) { }</code> ...&#10;
                        /// </summary>

                    }
                    this.on = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// secureConnection: <code>function (cleartextStream) {}</code> ...&#10;
                        /// clientError: <code>function (exception, securePair) { }</code> ...&#10;
                        /// newSession: <code>function (sessionId, sessionData) { }</code> ...&#10;
                        /// resumeSession: <code>function (sessionId, callback) { }</code> ...&#10;
                        /// </summary>

                    }
                    /// <field name='maxConnections'><p>Set this property to reject connections when the server&#39;s connection count&#10;gets high.&#10;&#10;</p>&#10;</field>
                    this.maxConnections = undefined;
                    /// <field name='connections'><p>The number of concurrent connections on the server.&#10;&#10;&#10;</p>&#10;</field>
                    this.connections = undefined;
                }

                this.Server = function() {
                    return new _Server();
                }
                function _CryptoStream() {
                    /// <field name='bytesWritten'><p>A proxy to the underlying socket&#39;s bytesWritten accessor, this will return&#10;the total bytes written to the socket, <em>including the TLS overhead</em>.&#10;&#10;</p>&#10;</field>
                    this.bytesWritten = undefined;
                }

                this.CryptoStream = function() {
                    return new _CryptoStream();
                }
                function _CleartextStream() {
                    this.getPeerCertificate = function() {
                        /// <summary><p>Returns an object representing the peer&#39;s certificate. The returned object has&#10;some properties corresponding to the field of the certificate.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>{ subject: &#10;   { C: &#39;UK&#39;,&#10;     ST: &#39;Acknack Ltd&#39;,&#10;     L: &#39;Rhys Jones&#39;,&#10;     O: &#39;node.js&#39;,&#10;     OU: &#39;Test TLS Certificate&#39;,&#10;     CN: &#39;localhost&#39; },&#10;  issuer: &#10;   { C: &#39;UK&#39;,&#10;     ST: &#39;Acknack Ltd&#39;,&#10;     L: &#39;Rhys Jones&#39;,&#10;     O: &#39;node.js&#39;,&#10;     OU: &#39;Test TLS Certificate&#39;,&#10;     CN: &#39;localhost&#39; },&#10;  valid_from: &#39;Nov 11 09:52:22 2009 GMT&#39;,&#10;  valid_to: &#39;Nov  6 09:52:22 2029 GMT&#39;,&#10;  fingerprint: &#39;2A:7A:C2:DD:E5:F9:CC:53:72:35:99:7A:02:5A:71:38:52:EC:8A:DF&#39; }</code></pre>&#10;<p>If the peer does not provide a certificate, it returns <code>null</code> or an empty&#10;object.&#10;&#10;</p>&#10;</summary>
                    }
                    this.getCipher = function() {
                        /// <summary><p>Returns an object representing the cipher name and the SSL/TLS&#10;protocol version of the current connection.&#10;&#10;</p>&#10;<p>Example:&#10;{ name: &#39;AES256-SHA&#39;, version: &#39;TLSv1/SSLv3&#39; }&#10;&#10;</p>&#10;<p>See SSL_CIPHER_get_name() and SSL_CIPHER_get_version() in&#10;<a href="http://www.openssl.org/docs/ssl/ssl.html#DEALING_WITH_CIPHERS">http://www.openssl.org/docs/ssl/ssl.html#DEALING_WITH_CIPHERS</a> for more&#10;information.&#10;&#10;</p>&#10;</summary>
                    }
                    this.address = function() {
                        /// <summary><p>Returns the bound address, the address family name and port of the&#10;underlying socket as reported by the operating system. Returns an&#10;object with three properties, e.g.&#10;<code>{ port: 12346, family: &#39;IPv4&#39;, address: &#39;127.0.0.1&#39; }</code>&#10;&#10;</p>&#10;</summary>
                    }
                    this.addListener = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// secureConnect: This event is emitted after a new connection has been successfully handshaked.  ...&#10;
                        /// </summary>

                    }
                    this.once = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// secureConnect: This event is emitted after a new connection has been successfully handshaked.  ...&#10;
                        /// </summary>

                    }
                    this.removeListener = function(event, listener) {
                        /// <summary>Supported Events: secureConnect</summary>
                    }
                    this.removeAllListeners = function(event) {
                        /// <summary>Supported Events: secureConnect</summary>
                    }
                    this.setMaxListeners = function(n) { }
                    this.listeners = function(event) {
                        /// <summary>Supported Events: secureConnect</summary>
                    }
                    this.emit = function(event, arguments) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// secureConnect: This event is emitted after a new connection has been successfully handshaked.  ...&#10;
                        /// </summary>

                    }
                    this.on = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// secureConnect: This event is emitted after a new connection has been successfully handshaked.  ...&#10;
                        /// </summary>

                    }
                    /// <field name='authorized'><p>A boolean that is <code>true</code> if the peer certificate was signed by one of the&#10;specified CAs, otherwise <code>false</code>&#10;&#10;</p>&#10;</field>
                    this.authorized = undefined;
                    /// <field name='authorizationError'><p>The reason why the peer&#39;s certificate has not been verified. This property&#10;becomes available only when <code>cleartextStream.authorized === false</code>.&#10;&#10;</p>&#10;</field>
                    this.authorizationError = undefined;
                    /// <field name='remoteAddress'><p>The string representation of the remote IP address. For example,&#10;<code>&#39;74.125.127.100&#39;</code> or <code>&#39;2001:4860:a005::68&#39;</code>.&#10;&#10;</p>&#10;</field>
                    this.remoteAddress = undefined;
                    /// <field name='remotePort'><p>The numeric representation of the remote port. For example, <code>443</code>.&#10;&#10;</p>&#10;</field>
                    this.remotePort = undefined;
                }

                this.CleartextStream = function() {
                    return new _CleartextStream();
                }
                /// <field name='SLAB_BUFFER_SIZE'><p>Size of slab buffer used by all tls servers and clients.&#10;Default: <code>10 * 1024 * 1024</code>.&#10;&#10;&#10;</p>&#10;<p>Don&#39;t change the defaults unless you know what you are doing.&#10;&#10;&#10;</p>&#10;</field>
                this.SLAB_BUFFER_SIZE = undefined;
            };
            case "stringdecoder": return new     function stringdecoder() {
                /// <summary><p>To use this module, do <code>require(&#39;string_decoder&#39;)</code>. StringDecoder decodes a&#10;buffer to a string. It is a simple interface to <code>buffer.toString()</code> but provides&#10;additional support for utf8.&#10;&#10;</p>&#10;<pre><code>var StringDecoder = require(&#39;string_decoder&#39;).StringDecoder;&#10;var decoder = new StringDecoder(&#39;utf8&#39;);&#10;&#10;var cent = new Buffer([0xC2, 0xA2]);&#10;console.log(decoder.write(cent));&#10;&#10;var euro = new Buffer([0xE2, 0x82, 0xAC]);&#10;console.log(decoder.write(euro));</code></pre>&#10;</summary>
                function _StringDecoder() {
                    this.write = function(buffer) {
                        /// <summary><p>Returns a decoded string.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="buffer"></param>
                        /// </signature>
                    }
                    this.end = function() {
                        /// <summary><p>Returns any trailing bytes that were left in the buffer.&#10;&#10;</p>&#10;</summary>
                    }
                }

                this.StringDecoder = function() {
                    return new _StringDecoder();
                }
            };
            case "fs": return new     function fs() {
                /// <summary><p>File I/O is provided by simple wrappers around standard POSIX functions.  To&#10;use this module do <code>require(&#39;fs&#39;)</code>. All the methods have asynchronous and&#10;synchronous forms.&#10;&#10;</p>&#10;<p>The asynchronous form always take a completion callback as its last argument.&#10;The arguments passed to the completion callback depend on the method, but the&#10;first argument is always reserved for an exception. If the operation was&#10;completed successfully, then the first argument will be <code>null</code> or <code>undefined</code>.&#10;&#10;</p>&#10;<p>When using the synchronous form any exceptions are immediately thrown.&#10;You can use try/catch to handle exceptions or allow them to bubble up.&#10;&#10;</p>&#10;<p>Here is an example of the asynchronous version:&#10;&#10;</p>&#10;<pre><code>var fs = require(&#39;fs&#39;);&#10;&#10;fs.unlink(&#39;/tmp/hello&#39;, function (err) {&#10;  if (err) throw err;&#10;  console.log(&#39;successfully deleted /tmp/hello&#39;);&#10;});</code></pre>&#10;<p>Here is the synchronous version:&#10;&#10;</p>&#10;<pre><code>var fs = require(&#39;fs&#39;);&#10;&#10;fs.unlinkSync(&#39;/tmp/hello&#39;)&#10;console.log(&#39;successfully deleted /tmp/hello&#39;);</code></pre>&#10;<p>With the asynchronous methods there is no guaranteed ordering. So the&#10;following is prone to error:&#10;&#10;</p>&#10;<pre><code>fs.rename(&#39;/tmp/hello&#39;, &#39;/tmp/world&#39;, function (err) {&#10;  if (err) throw err;&#10;  console.log(&#39;renamed complete&#39;);&#10;});&#10;fs.stat(&#39;/tmp/world&#39;, function (err, stats) {&#10;  if (err) throw err;&#10;  console.log(&#39;stats: &#39; + JSON.stringify(stats));&#10;});</code></pre>&#10;<p>It could be that <code>fs.stat</code> is executed before <code>fs.rename</code>.&#10;The correct way to do this is to chain the callbacks.&#10;&#10;</p>&#10;<pre><code>fs.rename(&#39;/tmp/hello&#39;, &#39;/tmp/world&#39;, function (err) {&#10;  if (err) throw err;&#10;  fs.stat(&#39;/tmp/world&#39;, function (err, stats) {&#10;    if (err) throw err;&#10;    console.log(&#39;stats: &#39; + JSON.stringify(stats));&#10;  });&#10;});</code></pre>&#10;<p>In busy processes, the programmer is <em>strongly encouraged</em> to use the&#10;asynchronous versions of these calls. The synchronous versions will block&#10;the entire process until they complete--halting all connections.&#10;&#10;</p>&#10;<p>Relative path to filename can be used, remember however that this path will be&#10;relative to <code>process.cwd()</code>.&#10;&#10;</p>&#10;<p>Most fs functions let you omit the callback argument. If you do, a default&#10;callback is used that ignores errors, but prints a deprecation&#10;warning.&#10;&#10;</p>&#10;<p><strong>IMPORTANT</strong>: Omitting the callback is deprecated.  v0.12 will throw the&#10;errors as exceptions.&#10;&#10;&#10;</p>&#10;</summary>
                this.rename = function(oldPath, newPath, callback) {
                    /// <summary><p>Asynchronous rename(2). No arguments other than a possible exception are given&#10;to the completion callback.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="oldPath"></param>
                    /// <param name="newPath"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.renameSync = function(oldPath, newPath) {
                    /// <summary><p>Synchronous rename(2).&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="oldPath"></param>
                    /// <param name="newPath"></param>
                    /// </signature>
                }
                this.ftruncate = function(fd, len, callback) {
                    /// <summary><p>Asynchronous ftruncate(2). No arguments other than a possible exception are&#10;given to the completion callback.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="fd"></param>
                    /// <param name="len"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.ftruncateSync = function(fd, len) {
                    /// <summary><p>Synchronous ftruncate(2).&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="fd"></param>
                    /// <param name="len"></param>
                    /// </signature>
                }
                this.truncate = function(path, len, callback) {
                    /// <summary><p>Asynchronous truncate(2). No arguments other than a possible exception are&#10;given to the completion callback.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="path"></param>
                    /// <param name="len"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.truncateSync = function(path, len) {
                    /// <summary><p>Synchronous truncate(2).&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="path"></param>
                    /// <param name="len"></param>
                    /// </signature>
                }
                this.chown = function(path, uid, gid, callback) {
                    /// <summary><p>Asynchronous chown(2). No arguments other than a possible exception are given&#10;to the completion callback.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="path"></param>
                    /// <param name="uid"></param>
                    /// <param name="gid"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.chownSync = function(path, uid, gid) {
                    /// <summary><p>Synchronous chown(2).&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="path"></param>
                    /// <param name="uid"></param>
                    /// <param name="gid"></param>
                    /// </signature>
                }
                this.fchown = function(fd, uid, gid, callback) {
                    /// <summary><p>Asynchronous fchown(2). No arguments other than a possible exception are given&#10;to the completion callback.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="fd"></param>
                    /// <param name="uid"></param>
                    /// <param name="gid"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.fchownSync = function(fd, uid, gid) {
                    /// <summary><p>Synchronous fchown(2).&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="fd"></param>
                    /// <param name="uid"></param>
                    /// <param name="gid"></param>
                    /// </signature>
                }
                this.lchown = function(path, uid, gid, callback) {
                    /// <summary><p>Asynchronous lchown(2). No arguments other than a possible exception are given&#10;to the completion callback.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="path"></param>
                    /// <param name="uid"></param>
                    /// <param name="gid"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.lchownSync = function(path, uid, gid) {
                    /// <summary><p>Synchronous lchown(2).&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="path"></param>
                    /// <param name="uid"></param>
                    /// <param name="gid"></param>
                    /// </signature>
                }
                this.chmod = function(path, mode, callback) {
                    /// <summary><p>Asynchronous chmod(2). No arguments other than a possible exception are given&#10;to the completion callback.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="path"></param>
                    /// <param name="mode"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.chmodSync = function(path, mode) {
                    /// <summary><p>Synchronous chmod(2).&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="path"></param>
                    /// <param name="mode"></param>
                    /// </signature>
                }
                this.fchmod = function(fd, mode, callback) {
                    /// <summary><p>Asynchronous fchmod(2). No arguments other than a possible exception&#10;are given to the completion callback.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="fd"></param>
                    /// <param name="mode"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.fchmodSync = function(fd, mode) {
                    /// <summary><p>Synchronous fchmod(2).&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="fd"></param>
                    /// <param name="mode"></param>
                    /// </signature>
                }
                this.lchmod = function(path, mode, callback) {
                    /// <summary><p>Asynchronous lchmod(2). No arguments other than a possible exception&#10;are given to the completion callback.&#10;&#10;</p>&#10;<p>Only available on Mac OS X.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="path"></param>
                    /// <param name="mode"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.lchmodSync = function(path, mode) {
                    /// <summary><p>Synchronous lchmod(2).&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="path"></param>
                    /// <param name="mode"></param>
                    /// </signature>
                }
                this.stat = function(path, callback) {
                    /// <summary><p>Asynchronous stat(2). The callback gets two arguments <code>(err, stats)</code> where&#10;<code>stats</code> is a <a href="#fs_class_fs_stats">fs.Stats</a> object.  See the <a href="#fs_class_fs_stats">fs.Stats</a>&#10;section below for more information.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="path"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.lstat = function(path, callback) {
                    /// <summary><p>Asynchronous lstat(2). The callback gets two arguments <code>(err, stats)</code> where&#10;<code>stats</code> is a <code>fs.Stats</code> object. <code>lstat()</code> is identical to <code>stat()</code>, except that if&#10;<code>path</code> is a symbolic link, then the link itself is stat-ed, not the file that it&#10;refers to.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="path"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.fstat = function(fd, callback) {
                    /// <summary><p>Asynchronous fstat(2). The callback gets two arguments <code>(err, stats)</code> where&#10;<code>stats</code> is a <code>fs.Stats</code> object. <code>fstat()</code> is identical to <code>stat()</code>, except that&#10;the file to be stat-ed is specified by the file descriptor <code>fd</code>.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="fd"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.statSync = function(path) {
                    /// <summary><p>Synchronous stat(2). Returns an instance of <code>fs.Stats</code>.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="path"></param>
                    /// </signature>
                }
                this.lstatSync = function(path) {
                    /// <summary><p>Synchronous lstat(2). Returns an instance of <code>fs.Stats</code>.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="path"></param>
                    /// </signature>
                }
                this.fstatSync = function(fd) {
                    /// <summary><p>Synchronous fstat(2). Returns an instance of <code>fs.Stats</code>.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="fd"></param>
                    /// </signature>
                }
                this.link = function(srcpath, dstpath, callback) {
                    /// <summary><p>Asynchronous link(2). No arguments other than a possible exception are given to&#10;the completion callback.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="srcpath"></param>
                    /// <param name="dstpath"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.linkSync = function(srcpath, dstpath) {
                    /// <summary><p>Synchronous link(2).&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="srcpath"></param>
                    /// <param name="dstpath"></param>
                    /// </signature>
                }
                this.symlink = function(srcpath, dstpath, type, callback) {
                    /// <summary><p>Asynchronous symlink(2). No arguments other than a possible exception are given&#10;to the completion callback.&#10;<code>type</code> argument can be either <code>&#39;dir&#39;</code>, <code>&#39;file&#39;</code>, or <code>&#39;junction&#39;</code> (default is <code>&#39;file&#39;</code>).  It is only &#10;used on Windows (ignored on other platforms).&#10;Note that Windows junction points require the destination path to be absolute.  When using&#10;<code>&#39;junction&#39;</code>, the <code>destination</code> argument will automatically be normalized to absolute path.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="srcpath"></param>
                    /// <param name="dstpath"></param>
                    /// <param name="type"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.symlinkSync = function(srcpath, dstpath, type) {
                    /// <summary><p>Synchronous symlink(2).&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="srcpath"></param>
                    /// <param name="dstpath"></param>
                    /// <param name="type"></param>
                    /// </signature>
                }
                this.readlink = function(path, callback) {
                    /// <summary><p>Asynchronous readlink(2). The callback gets two arguments <code>(err,&#10;linkString)</code>.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="path"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.readlinkSync = function(path) {
                    /// <summary><p>Synchronous readlink(2). Returns the symbolic link&#39;s string value.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="path"></param>
                    /// </signature>
                }
                this.realpath = function(path, cache, callback) {
                    /// <summary><p>Asynchronous realpath(2). The <code>callback</code> gets two arguments <code>(err,&#10;resolvedPath)</code>. May use <code>process.cwd</code> to resolve relative paths. <code>cache</code> is an&#10;object literal of mapped paths that can be used to force a specific path&#10;resolution or avoid additional <code>fs.stat</code> calls for known real paths.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>var cache = {&#39;/etc&#39;:&#39;/private/etc&#39;};&#10;fs.realpath(&#39;/etc/passwd&#39;, cache, function (err, resolvedPath) {&#10;  if (err) throw err;&#10;  console.log(resolvedPath);&#10;});</code></pre>&#10;</summary>
                    /// <signature>
                    /// <param name="path"></param>
                    /// <param name="cache"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.realpathSync = function(path, cache) {
                    /// <summary><p>Synchronous realpath(2). Returns the resolved path.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="path"></param>
                    /// <param name="cache"></param>
                    /// </signature>
                }
                this.unlink = function(path, callback) {
                    /// <summary><p>Asynchronous unlink(2). No arguments other than a possible exception are given&#10;to the completion callback.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="path"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.unlinkSync = function(path) {
                    /// <summary><p>Synchronous unlink(2).&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="path"></param>
                    /// </signature>
                }
                this.rmdir = function(path, callback) {
                    /// <summary><p>Asynchronous rmdir(2). No arguments other than a possible exception are given&#10;to the completion callback.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="path"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.rmdirSync = function(path) {
                    /// <summary><p>Synchronous rmdir(2).&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="path"></param>
                    /// </signature>
                }
                this.mkdir = function(path, mode, callback) {
                    /// <summary><p>Asynchronous mkdir(2). No arguments other than a possible exception are given&#10;to the completion callback. <code>mode</code> defaults to <code>0777</code>.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="path"></param>
                    /// <param name="mode"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.mkdirSync = function(path, mode) {
                    /// <summary><p>Synchronous mkdir(2).&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="path"></param>
                    /// <param name="mode"></param>
                    /// </signature>
                }
                this.readdir = function(path, callback) {
                    /// <summary><p>Asynchronous readdir(3).  Reads the contents of a directory.&#10;The callback gets two arguments <code>(err, files)</code> where <code>files</code> is an array of&#10;the names of the files in the directory excluding <code>&#39;.&#39;</code> and <code>&#39;..&#39;</code>.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="path"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.readdirSync = function(path) {
                    /// <summary><p>Synchronous readdir(3). Returns an array of filenames excluding <code>&#39;.&#39;</code> and&#10;<code>&#39;..&#39;</code>.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="path"></param>
                    /// </signature>
                }
                this.close = function(fd, callback) {
                    /// <summary><p>Asynchronous close(2).  No arguments other than a possible exception are given&#10;to the completion callback.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="fd"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.closeSync = function(fd) {
                    /// <summary><p>Synchronous close(2).&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="fd"></param>
                    /// </signature>
                }
                this.open = function(path, flags, mode, callback) {
                    /// <summary><p>Asynchronous file open. See open(2). <code>flags</code> can be:&#10;&#10;</p>&#10;<ul>&#10;<li><p><code>&#39;r&#39;</code> - Open file for reading.&#10;An exception occurs if the file does not exist.</p>&#10;</li>&#10;<li><p><code>&#39;r+&#39;</code> - Open file for reading and writing.&#10;An exception occurs if the file does not exist.</p>&#10;</li>&#10;<li><p><code>&#39;rs&#39;</code> - Open file for reading in synchronous mode. Instructs the operating&#10;system to bypass the local file system cache.</p>&#10;<p>This is primarily useful for opening files on NFS mounts as it allows you to&#10;skip the potentially stale local cache. It has a very real impact on I/O&#10;performance so don&#39;t use this flag unless you need it.</p>&#10;<p>Note that this doesn&#39;t turn <code>fs.open()</code> into a synchronous blocking call.&#10;If that&#39;s what you want then you should be using <code>fs.openSync()</code></p>&#10;</li>&#10;<li><p><code>&#39;rs+&#39;</code> - Open file for reading and writing, telling the OS to open it&#10;synchronously. See notes for <code>&#39;rs&#39;</code> about using this with caution.</p>&#10;</li>&#10;<li><p><code>&#39;w&#39;</code> - Open file for writing.&#10;The file is created (if it does not exist) or truncated (if it exists).</p>&#10;</li>&#10;<li><p><code>&#39;wx&#39;</code> - Like <code>&#39;w&#39;</code> but fails if <code>path</code> exists.</p>&#10;</li>&#10;<li><p><code>&#39;w+&#39;</code> - Open file for reading and writing.&#10;The file is created (if it does not exist) or truncated (if it exists).</p>&#10;</li>&#10;<li><p><code>&#39;wx+&#39;</code> - Like <code>&#39;w+&#39;</code> but fails if <code>path</code> exists.</p>&#10;</li>&#10;<li><p><code>&#39;a&#39;</code> - Open file for appending.&#10;The file is created if it does not exist.</p>&#10;</li>&#10;<li><p><code>&#39;ax&#39;</code> - Like <code>&#39;a&#39;</code> but fails if <code>path</code> exists.</p>&#10;</li>&#10;<li><p><code>&#39;a+&#39;</code> - Open file for reading and appending.&#10;The file is created if it does not exist.</p>&#10;</li>&#10;<li><p><code>&#39;ax+&#39;</code> - Like <code>&#39;a+&#39;</code> but fails if <code>path</code> exists.</p>&#10;</li>&#10;</ul>&#10;<p><code>mode</code> sets the file mode (permission and sticky bits), but only if the file was&#10;created. It defaults to <code>0666</code>, readable and writeable.&#10;&#10;</p>&#10;<p>The callback gets two arguments <code>(err, fd)</code>.&#10;&#10;</p>&#10;<p>The exclusive flag <code>&#39;x&#39;</code> (<code>O_EXCL</code> flag in open(2)) ensures that <code>path</code> is newly&#10;created. On POSIX systems, <code>path</code> is considered to exist even if it is a symlink&#10;to a non-existent file. The exclusive flag may or may not work with network file&#10;systems.&#10;&#10;</p>&#10;<p>On Linux, positional writes don&#39;t work when the file is opened in append mode.&#10;The kernel ignores the position argument and always appends the data to&#10;the end of the file.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="path"></param>
                    /// <param name="flags"></param>
                    /// <param name="mode"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.openSync = function(path, flags, mode) {
                    /// <summary><p>Synchronous version of <code>fs.open()</code>.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="path"></param>
                    /// <param name="flags"></param>
                    /// <param name="mode"></param>
                    /// </signature>
                }
                this.utimes = function(path, atime, mtime) {
                    /// <summary><p>Change file timestamps of the file referenced by the supplied path.&#10;&#10;</p>&#10;</summary>
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
                    /// <summary><p>Change file timestamps of the file referenced by the supplied path.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="path"></param>
                    /// <param name="atime"></param>
                    /// <param name="mtime"></param>
                    /// </signature>
                }
                this.futimes = function(fd, atime, mtime) {
                    /// <summary><p>Change the file timestamps of a file referenced by the supplied file&#10;descriptor.&#10;&#10;</p>&#10;</summary>
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
                    /// <summary><p>Change the file timestamps of a file referenced by the supplied file&#10;descriptor.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="fd"></param>
                    /// <param name="atime"></param>
                    /// <param name="mtime"></param>
                    /// </signature>
                }
                this.fsync = function(fd, callback) {
                    /// <summary><p>Asynchronous fsync(2). No arguments other than a possible exception are given&#10;to the completion callback.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="fd"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.fsyncSync = function(fd) {
                    /// <summary><p>Synchronous fsync(2).&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="fd"></param>
                    /// </signature>
                }
                this.write = function(fd, buffer, offset, length, position, callback) {
                    /// <summary><p>Write <code>buffer</code> to the file specified by <code>fd</code>.&#10;&#10;</p>&#10;<p><code>offset</code> and <code>length</code> determine the part of the buffer to be written.&#10;&#10;</p>&#10;<p><code>position</code> refers to the offset from the beginning of the file where this data&#10;should be written. If <code>position</code> is <code>null</code>, the data will be written at the&#10;current position.&#10;See pwrite(2).&#10;&#10;</p>&#10;<p>The callback will be given three arguments <code>(err, written, buffer)</code> where <code>written</code>&#10;specifies how many <em>bytes</em> were written from <code>buffer</code>.&#10;&#10;</p>&#10;<p>Note that it is unsafe to use <code>fs.write</code> multiple times on the same file&#10;without waiting for the callback. For this scenario,&#10;<code>fs.createWriteStream</code> is strongly recommended.&#10;&#10;</p>&#10;<p>On Linux, positional writes don&#39;t work when the file is opened in append mode.&#10;The kernel ignores the position argument and always appends the data to&#10;the end of the file.&#10;&#10;</p>&#10;</summary>
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
                    /// <summary><p>Synchronous version of <code>fs.write()</code>. Returns the number of bytes written.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="fd"></param>
                    /// <param name="buffer"></param>
                    /// <param name="offset"></param>
                    /// <param name="length"></param>
                    /// <param name="position"></param>
                    /// </signature>
                }
                this.read = function(fd, buffer, offset, length, position, callback) {
                    /// <summary><p>Read data from the file specified by <code>fd</code>.&#10;&#10;</p>&#10;<p><code>buffer</code> is the buffer that the data will be written to.&#10;&#10;</p>&#10;<p><code>offset</code> is the offset in the buffer to start writing at.&#10;&#10;</p>&#10;<p><code>length</code> is an integer specifying the number of bytes to read.&#10;&#10;</p>&#10;<p><code>position</code> is an integer specifying where to begin reading from in the file.&#10;If <code>position</code> is <code>null</code>, data will be read from the current file position.&#10;&#10;</p>&#10;<p>The callback is given the three arguments, <code>(err, bytesRead, buffer)</code>.&#10;&#10;</p>&#10;</summary>
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
                    /// <summary><p>Synchronous version of <code>fs.read</code>. Returns the number of <code>bytesRead</code>.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="fd"></param>
                    /// <param name="buffer"></param>
                    /// <param name="offset"></param>
                    /// <param name="length"></param>
                    /// <param name="position"></param>
                    /// </signature>
                }
                this.readFile = function(filename, options, callback) {
                    /// <summary><p>Asynchronously reads the entire contents of a file. Example:&#10;&#10;</p>&#10;<pre><code>fs.readFile(&#39;/etc/passwd&#39;, function (err, data) {&#10;  if (err) throw err;&#10;  console.log(data);&#10;});</code></pre>&#10;<p>The callback is passed two arguments <code>(err, data)</code>, where <code>data</code> is the&#10;contents of the file.&#10;&#10;</p>&#10;<p>If no encoding is specified, then the raw buffer is returned.&#10;&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="filename" type="String"></param>
                    /// <param name="options" type="Object"></param>
                    /// <param name="callback" type="Function"></param>
                    /// </signature>
                    /// <signature>
                    /// <param name="filename"></param>
                    /// <param name="options"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.readFileSync = function(filename, options) {
                    /// <summary><p>Synchronous version of <code>fs.readFile</code>. Returns the contents of the <code>filename</code>.&#10;&#10;</p>&#10;<p>If the <code>encoding</code> option is specified then this function returns a&#10;string. Otherwise it returns a buffer.&#10;&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="filename"></param>
                    /// <param name="options"></param>
                    /// </signature>
                }
                this.writeFile = function(filename, data, options, callback) {
                    /// <summary><p>Asynchronously writes data to a file, replacing the file if it already exists.&#10;<code>data</code> can be a string or a buffer.&#10;&#10;</p>&#10;<p>The <code>encoding</code> option is ignored if <code>data</code> is a buffer. It defaults&#10;to <code>&#39;utf8&#39;</code>.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>fs.writeFile(&#39;message.txt&#39;, &#39;Hello Node&#39;, function (err) {&#10;  if (err) throw err;&#10;  console.log(&#39;It\&#39;s saved!&#39;);&#10;});</code></pre>&#10;</summary>
                    /// <signature>
                    /// <param name="filename" type="String"></param>
                    /// <param name="data" type="String"></param>
                    /// <param name="options" type="Object"></param>
                    /// <param name="callback" type="Function"></param>
                    /// </signature>
                    /// <signature>
                    /// <param name="filename"></param>
                    /// <param name="data"></param>
                    /// <param name="options"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.writeFileSync = function(filename, data, options) {
                    /// <summary><p>The synchronous version of <code>fs.writeFile</code>.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="filename"></param>
                    /// <param name="data"></param>
                    /// <param name="options"></param>
                    /// </signature>
                }
                this.appendFile = function(filename, data, options, callback) {
                    /// <summary><p>Asynchronously append data to a file, creating the file if it not yet exists.&#10;<code>data</code> can be a string or a buffer.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>fs.appendFile(&#39;message.txt&#39;, &#39;data to append&#39;, function (err) {&#10;  if (err) throw err;&#10;  console.log(&#39;The &quot;data to append&quot; was appended to file!&#39;);&#10;});</code></pre>&#10;</summary>
                    /// <signature>
                    /// <param name="filename" type="String"></param>
                    /// <param name="data" type="String"></param>
                    /// <param name="options" type="Object"></param>
                    /// <param name="callback" type="Function"></param>
                    /// </signature>
                    /// <signature>
                    /// <param name="filename"></param>
                    /// <param name="data"></param>
                    /// <param name="options"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.appendFileSync = function(filename, data, options) {
                    /// <summary><p>The synchronous version of <code>fs.appendFile</code>.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="filename"></param>
                    /// <param name="data"></param>
                    /// <param name="options"></param>
                    /// </signature>
                }
                this.watchFile = function(filename, options, listener) {
                    /// <summary><p>Watch for changes on <code>filename</code>. The callback <code>listener</code> will be called each&#10;time the file is accessed.&#10;&#10;</p>&#10;<p>The second argument is optional. The <code>options</code> if provided should be an object&#10;containing two members a boolean, <code>persistent</code>, and <code>interval</code>. <code>persistent</code>&#10;indicates whether the process should continue to run as long as files are&#10;being watched. <code>interval</code> indicates how often the target should be polled,&#10;in milliseconds. The default is <code>{ persistent: true, interval: 5007 }</code>.&#10;&#10;</p>&#10;<p>The <code>listener</code> gets two arguments the current stat object and the previous&#10;stat object:&#10;&#10;</p>&#10;<pre><code>fs.watchFile(&#39;message.text&#39;, function (curr, prev) {&#10;  console.log(&#39;the current mtime is: &#39; + curr.mtime);&#10;  console.log(&#39;the previous mtime was: &#39; + prev.mtime);&#10;});</code></pre>&#10;<p>These stat objects are instances of <code>fs.Stat</code>.&#10;&#10;</p>&#10;<p>If you want to be notified when the file was modified, not just accessed&#10;you need to compare <code>curr.mtime</code> and <code>prev.mtime</code>.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="filename"></param>
                    /// <param name="options"></param>
                    /// <param name="listener"></param>
                    /// </signature>
                }
                this.unwatchFile = function(filename, listener) {
                    /// <summary><p>Stop watching for changes on <code>filename</code>. If <code>listener</code> is specified, only that&#10;particular listener is removed. Otherwise, <em>all</em> listeners are removed and you&#10;have effectively stopped watching <code>filename</code>.&#10;&#10;</p>&#10;<p>Calling <code>fs.unwatchFile()</code> with a filename that is not being watched is a&#10;no-op, not an error.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="filename"></param>
                    /// <param name="listener"></param>
                    /// </signature>
                }
                this.watch = function(filename, options, listener) {
                    /// <summary><p>Watch for changes on <code>filename</code>, where <code>filename</code> is either a file or a&#10;directory.  The returned object is a <a href="#fs_class_fs_fswatcher">fs.FSWatcher</a>.&#10;&#10;</p>&#10;<p>The second argument is optional. The <code>options</code> if provided should be an object&#10;containing a boolean member <code>persistent</code>, which indicates whether the process&#10;should continue to run as long as files are being watched. The default is&#10;<code>{ persistent: true }</code>.&#10;&#10;</p>&#10;<p>The listener callback gets two arguments <code>(event, filename)</code>.  <code>event</code> is either&#10;&#39;rename&#39; or &#39;change&#39;, and <code>filename</code> is the name of the file which triggered&#10;the event.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="filename"></param>
                    /// <param name="options"></param>
                    /// <param name="listener"></param>
                    /// </signature>
                }
                this.exists = function(path, callback) {
                    /// <summary><p>Test whether or not the given path exists by checking with the file system.&#10;Then call the <code>callback</code> argument with either true or false.  Example:&#10;&#10;</p>&#10;<pre><code>fs.exists(&#39;/etc/passwd&#39;, function (exists) {&#10;  util.debug(exists ? &quot;it&#39;s there&quot; : &quot;no passwd!&quot;);&#10;});</code></pre>&#10;</summary>
                    /// <signature>
                    /// <param name="path"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.existsSync = function(path) {
                    /// <summary><p>Synchronous version of <code>fs.exists</code>.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="path"></param>
                    /// </signature>
                }
                this.createReadStream = function(path, options) {
                    /// <summary><p>Returns a new ReadStream object (See <code>Readable Stream</code>).&#10;&#10;</p>&#10;<p><code>options</code> is an object with the following defaults:&#10;&#10;</p>&#10;<pre><code>{ flags: &#39;r&#39;,&#10;  encoding: null,&#10;  fd: null,&#10;  mode: 0666,&#10;  autoClose: true&#10;}</code></pre>&#10;<p><code>options</code> can include <code>start</code> and <code>end</code> values to read a range of bytes from&#10;the file instead of the entire file.  Both <code>start</code> and <code>end</code> are inclusive and&#10;start at 0. The <code>encoding</code> can be <code>&#39;utf8&#39;</code>, <code>&#39;ascii&#39;</code>, or <code>&#39;base64&#39;</code>.&#10;&#10;</p>&#10;<p>If <code>autoClose</code> is false, then the file descriptor won&#39;t be closed, even if&#10;there&#39;s an error.  It is your responsiblity to close it and make sure&#10;there&#39;s no file descriptor leak.  If <code>autoClose</code> is set to true (default&#10;behavior), on <code>error</code> or <code>end</code> the file descriptor will be closed&#10;automatically.&#10;&#10;</p>&#10;<p>An example to read the last 10 bytes of a file which is 100 bytes long:&#10;&#10;</p>&#10;<pre><code>fs.createReadStream(&#39;sample.txt&#39;, {start: 90, end: 99});</code></pre>&#10;</summary>
                    /// <signature>
                    /// <param name="path"></param>
                    /// <param name="options"></param>
                    /// </signature>
                    return new this.ReadStream();
                }
                this.createWriteStream = function(path, options) {
                    /// <summary><p>Returns a new WriteStream object (See <code>Writable Stream</code>).&#10;&#10;</p>&#10;<p><code>options</code> is an object with the following defaults:&#10;&#10;</p>&#10;<pre><code>{ flags: &#39;w&#39;,&#10;  encoding: null,&#10;  mode: 0666 }</code></pre>&#10;<p><code>options</code> may also include a <code>start</code> option to allow writing data at&#10;some position past the beginning of the file.  Modifying a file rather&#10;than replacing it may require a <code>flags</code> mode of <code>r+</code> rather than the&#10;default mode <code>w</code>.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="path"></param>
                    /// <param name="options"></param>
                    /// </signature>
                    return new this.WriteStream();
                }
                function _Stats() {
                }

                this.Stats = function() {
                    return new _Stats();
                }
                function _ReadStream() {
                    this.addListener = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// open: Emitted when the ReadStream&#39;s file is opened. ...&#10;
                        /// </summary>

                    }
                    this.once = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// open: Emitted when the ReadStream&#39;s file is opened. ...&#10;
                        /// </summary>

                    }
                    this.removeListener = function(event, listener) {
                        /// <summary>Supported Events: open</summary>
                    }
                    this.removeAllListeners = function(event) {
                        /// <summary>Supported Events: open</summary>
                    }
                    this.setMaxListeners = function(n) { }
                    this.listeners = function(event) {
                        /// <summary>Supported Events: open</summary>
                    }
                    this.emit = function(event, arguments) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// open: Emitted when the ReadStream&#39;s file is opened. ...&#10;
                        /// </summary>

                    }
                    this.on = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// open: Emitted when the ReadStream&#39;s file is opened. ...&#10;
                        /// </summary>

                    }
                }

                this.ReadStream = function() {
                    return new _ReadStream();
                }
                function _WriteStream() {
                    this.addListener = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// open: Emitted when the WriteStream&#39;s file is opened. ...&#10;
                        /// </summary>

                    }
                    this.once = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// open: Emitted when the WriteStream&#39;s file is opened. ...&#10;
                        /// </summary>

                    }
                    this.removeListener = function(event, listener) {
                        /// <summary>Supported Events: open</summary>
                    }
                    this.removeAllListeners = function(event) {
                        /// <summary>Supported Events: open</summary>
                    }
                    this.setMaxListeners = function(n) { }
                    this.listeners = function(event) {
                        /// <summary>Supported Events: open</summary>
                    }
                    this.emit = function(event, arguments) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// open: Emitted when the WriteStream&#39;s file is opened. ...&#10;
                        /// </summary>

                    }
                    this.on = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// open: Emitted when the WriteStream&#39;s file is opened. ...&#10;
                        /// </summary>

                    }
                    /// <field name='bytesWritten'><p>The number of bytes written so far. Does not include data that is still queued&#10;for writing.&#10;&#10;</p>&#10;</field>
                    this.bytesWritten = undefined;
                }

                this.WriteStream = function() {
                    return new _WriteStream();
                }
                function _FSWatcher() {
                    this.close = function() {
                        /// <summary><p>Stop watching for changes on the given <code>fs.FSWatcher</code>.&#10;&#10;</p>&#10;</summary>
                    }
                    this.addListener = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// change: Emitted when something changes in a watched directory or file. ...&#10;
                        /// error: Emitted when an error occurs. ...&#10;
                        /// </summary>

                    }
                    this.once = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// change: Emitted when something changes in a watched directory or file. ...&#10;
                        /// error: Emitted when an error occurs. ...&#10;
                        /// </summary>

                    }
                    this.removeListener = function(event, listener) {
                        /// <summary>Supported Events: change, error</summary>
                    }
                    this.removeAllListeners = function(event) {
                        /// <summary>Supported Events: change, error</summary>
                    }
                    this.setMaxListeners = function(n) { }
                    this.listeners = function(event) {
                        /// <summary>Supported Events: change, error</summary>
                    }
                    this.emit = function(event, arguments) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// change: Emitted when something changes in a watched directory or file. ...&#10;
                        /// error: Emitted when an error occurs. ...&#10;
                        /// </summary>

                    }
                    this.on = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// change: Emitted when something changes in a watched directory or file. ...&#10;
                        /// error: Emitted when an error occurs. ...&#10;
                        /// </summary>

                    }
                }

                this.FSWatcher = function() {
                    return new _FSWatcher();
                }
            };
            case "path": return new     function path() {
                /// <summary><p>This module contains utilities for handling and transforming file&#10;paths.  Almost all these methods perform only string transformations.&#10;The file system is not consulted to check whether paths are valid.&#10;&#10;</p>&#10;<p>Use <code>require(&#39;path&#39;)</code> to use this module.  The following methods are provided:&#10;&#10;</p>&#10;</summary>
                this.normalize = function(p) {
                    /// <summary><p>Normalize a string path, taking care of <code>&#39;..&#39;</code> and <code>&#39;.&#39;</code> parts.&#10;&#10;</p>&#10;<p>When multiple slashes are found, they&#39;re replaced by a single one;&#10;when the path contains a trailing slash, it is preserved.&#10;On Windows backslashes are used.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>path.normalize(&#39;/foo/bar//baz/asdf/quux/..&#39;)&#10;// returns&#10;&#39;/foo/bar/baz/asdf&#39;</code></pre>&#10;</summary>
                    /// <signature>
                    /// <param name="p"></param>
                    /// </signature>

                    var parts = p.replace(/\\/g, '/').split('/');
                    var res = [];
                    for(var part = 0; part<parts.length; part++) {    
                        var partText = parts[part];

                        if ((partText == '' && part != 0)|| partText == '.') {
                            continue;
                        } else if(partText == '..' && res.length > 0) {
                            res.splice(res.length - 1, 1);
                        } else {
                            res[res.length] = partText;
                        }
                    }
                    var normalized = res.join('\\');
                    if(p[p.length - 1] == '/' || p[p.length - 1] == '\\') {
                        normalized += '\\';
                    }
                    return normalized;
                }
                this.join = function(path1, path2) {
                    /// <summary><p>Join all arguments together and normalize the resulting path.&#10;&#10;</p>&#10;<p>Arguments must be strings.  In v0.8, non-string arguments were&#10;silently ignored.  In v0.10 and up, an exception is thrown.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>path.join(&#39;/foo&#39;, &#39;bar&#39;, &#39;baz/asdf&#39;, &#39;quux&#39;, &#39;..&#39;)&#10;// returns&#10;&#39;/foo/bar/baz/asdf&#39;&#10;&#10;path.join(&#39;foo&#39;, {}, &#39;bar&#39;)&#10;// throws exception&#10;TypeError: Arguments to path.join must be strings</code></pre>&#10;</summary>
                    /// <signature>
                    /// <param name="path1"></param>
                    /// <param name="path2"></param>
                    /// <param name="..."></param>
                    /// </signature>
    
                    var args = Array.prototype.slice.call(arguments);
                    for (var i = 0; i<args.length; i++) {
                        if (args[i] == '') {
                            args.splice(i, 1);
                        }
                    }
                    return this.normalize(args.join('//'));

                }
                this.resolve = function(from, to) {
                    /// <summary><p>Resolves <code>to</code> to an absolute path.&#10;&#10;</p>&#10;<p>If <code>to</code> isn&#39;t already absolute <code>from</code> arguments are prepended in right to left&#10;order, until an absolute path is found. If after using all <code>from</code> paths still&#10;no absolute path is found, the current working directory is used as well. The&#10;resulting path is normalized, and trailing slashes are removed unless the path&#10;gets resolved to the root directory. Non-string arguments are ignored.&#10;&#10;</p>&#10;<p>Another way to think of it is as a sequence of <code>cd</code> commands in a shell.&#10;&#10;</p>&#10;<pre><code>path.resolve(&#39;foo/bar&#39;, &#39;/tmp/file/&#39;, &#39;..&#39;, &#39;a/../subfile&#39;)</code></pre>&#10;<p>Is similar to:&#10;&#10;</p>&#10;<pre><code>cd foo/bar&#10;cd /tmp/file/&#10;cd ..&#10;cd a/../subfile&#10;pwd</code></pre>&#10;<p>The difference is that the different paths don&#39;t need to exist and may also be&#10;files.&#10;&#10;</p>&#10;<p>Examples:&#10;&#10;</p>&#10;<pre><code>path.resolve(&#39;/foo/bar&#39;, &#39;./baz&#39;)&#10;// returns&#10;&#39;/foo/bar/baz&#39;&#10;&#10;path.resolve(&#39;/foo/bar&#39;, &#39;/tmp/file/&#39;)&#10;// returns&#10;&#39;/tmp/file&#39;&#10;&#10;path.resolve(&#39;wwwroot&#39;, &#39;static_files/png/&#39;, &#39;../gif/image.gif&#39;)&#10;// if currently in /home/myself/node, it returns&#10;&#39;/home/myself/node/wwwroot/static_files/gif/image.gif&#39;</code></pre>&#10;</summary>
                    /// <signature>
                    /// <param name="from ..."></param>
                    /// <param name="to"></param>
                    /// </signature>

                    var realTo = '';
                    for(var i = arguments.length - 1; i>= 0; i--) {
                        realTo = this.join(arguments[i], realTo);
                        if(realTo[0] == '/' || realTo[0] == '\\') {
                            // relative to drive which is available via __dirname
                            return __dirname.substr(0, 2) + realTo;
                        } else if((realTo[0].toUpperCase() >= 'A' && realTo[0].toUpperCase() <= 'Z')  && 
                                  realTo[1] == ':' &&
                                  (realTo[2] == '\\' || realTo[2] == '/')) {
                            // absolute path
                            return realTo;
                        }
                    }
                    var res = this.join(__dirname, realTo);
                    return res;
                }
                this.relative = function(from, to) {
                    /// <summary><p>Solve the relative path from <code>from</code> to <code>to</code>.&#10;&#10;</p>&#10;<p>At times we have two absolute paths, and we need to derive the relative&#10;path from one to the other.  This is actually the reverse transform of&#10;<code>path.resolve</code>, which means we see that:&#10;&#10;</p>&#10;<pre><code>path.resolve(from, path.relative(from, to)) == path.resolve(to)</code></pre>&#10;<p>Examples:&#10;&#10;</p>&#10;<pre><code>path.relative(&#39;C:\\orandea\\test\\aaa&#39;, &#39;C:\\orandea\\impl\\bbb&#39;)&#10;// returns&#10;&#39;..\\..\\impl\\bbb&#39;&#10;&#10;path.relative(&#39;/data/orandea/test/aaa&#39;, &#39;/data/orandea/impl/bbb&#39;)&#10;// returns&#10;&#39;../../impl/bbb&#39;</code></pre>&#10;</summary>
                    /// <signature>
                    /// <param name="from"></param>
                    /// <param name="to"></param>
                    /// </signature>
    
                    // switch to forward slashes
                    from = from.replace(/\\/g, '/');
                    to = to.replace(/\\/g, '/');
                    function fix_return(inp) {
                        if(inp[inp.length - 1] == '/') {
                            inp = inp.substr(0, inp.length - 1);
                        }
                        return inp.replace(/\//g, '\\');;
                    }

                    // fixup drive letters like Node
                    if(from[1] == ':') {
                        if(to[1] == ':') {
                            if(from[0].toLowerCase() != to[0].toLowerCase()) {
                                if(to[2] != '/') {
                                    to = __dirname.replace(/\\/g, '/') + '/' + to.substr(2);
                                }
                                // fully qualified on different drives
                                return fix_return(to);
                            }
                            if(to[2] != '/') {
                                to = __dirname.replace(/\\/g, '/') + '/' + to.substr(2);
                            }
                        } else {
                            if(to[0] != '/') {
                                to = '/' + to;
                            }
                            to = __dirname.substr(0, 2) + to;
                        }

                        if(from[2] != '/' && from[0] == __dirname[0]) {
                            from = __dirname.replace(/\\/g, '/') + '/' + from.substr(2);
                        }
                    } else if(to[1] == ':') {
                        from = __dirname.substr(0, 2) + from;
                        if(to[2] != '/') {
                            to = __dirname.replace(/\\/g, '/') + '/' + to.substr(2);
                        }
                    }

                    // normalize path endings
                    if(from[from.length - 1] != '/') {
                        from += '/';
                    }
                    if(to[to.length - 1] != '/') {
                        to += '/';
                    }    

                    // compare the paths
                    var i, si = -1;
                    for(i = 0; i<from.length && i < to.length; i++) {
                        if(from[i].toLowerCase() != to[i].toLowerCase()) {
                            break;
                        } else if(from[i] == '/') {
                            si = i;
                        }
                    }
                    if(si == -1 && to[1] == ':') {
                        return fix_return(to);
                    } else if(i == from.length && i == to.length) {
                        return '';
                    }
                    var res = '';
                    for(; i<from.length; i++) {
                        if(from[i] == '/') {
                            res += '../';
                        }
                    }
                    if(res.length == 0 && to.length - 1 == si) {
                        return './';
                    }
                    return fix_return(res + to.substr(si + 1));

                }
                this.dirname = function(p) {
                    /// <summary><p>Return the directory name of a path.  Similar to the Unix <code>dirname</code> command.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>path.dirname(&#39;/foo/bar/baz/asdf/quux&#39;)&#10;// returns&#10;&#39;/foo/bar/baz/asdf&#39;</code></pre>&#10;</summary>
                    /// <signature>
                    /// <param name="p"></param>
                    /// </signature>
                }
                this.basename = function(p, ext) {
                    /// <summary><p>Return the last portion of a path.  Similar to the Unix <code>basename</code> command.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>path.basename(&#39;/foo/bar/baz/asdf/quux.html&#39;)&#10;// returns&#10;&#39;quux.html&#39;&#10;&#10;path.basename(&#39;/foo/bar/baz/asdf/quux.html&#39;, &#39;.html&#39;)&#10;// returns&#10;&#39;quux&#39;</code></pre>&#10;</summary>
                    /// <signature>
                    /// <param name="p"></param>
                    /// <param name="ext"></param>
                    /// </signature>
                }
                this.extname = function(p) {
                    /// <summary><p>Return the extension of the path, from the last &#39;.&#39; to end of string&#10;in the last portion of the path.  If there is no &#39;.&#39; in the last portion&#10;of the path or the first character of it is &#39;.&#39;, then it returns&#10;an empty string.  Examples:&#10;&#10;</p>&#10;<pre><code>path.extname(&#39;index.html&#39;)&#10;// returns&#10;&#39;.html&#39;&#10;&#10;path.extname(&#39;index.&#39;)&#10;// returns&#10;&#39;.&#39;&#10;&#10;path.extname(&#39;index&#39;)&#10;// returns&#10;&#39;&#39;</code></pre>&#10;</summary>
                    /// <signature>
                    /// <param name="p"></param>
                    /// </signature>
                }
                /// <field name='sep'><p>The platform-specific file separator. <code>&#39;\\&#39;</code> or <code>&#39;/&#39;</code>.&#10;&#10;</p>&#10;<p>An example on *nix:&#10;&#10;</p>&#10;<pre><code>&#39;foo/bar/baz&#39;.split(path.sep)&#10;// returns&#10;[&#39;foo&#39;, &#39;bar&#39;, &#39;baz&#39;]</code></pre>&#10;<p>An example on Windows:&#10;&#10;</p>&#10;<pre><code>&#39;foo\\bar\\baz&#39;.split(path.sep)&#10;// returns&#10;[&#39;foo&#39;, &#39;bar&#39;, &#39;baz&#39;]</code></pre>&#10;</field>
                this.sep = undefined;
                /// <field name='delimiter'><p>The platform-specific path delimiter, <code>;</code> or <code>&#39;:&#39;</code>.&#10;&#10;</p>&#10;<p>An example on *nix:&#10;&#10;</p>&#10;<pre><code>console.log(process.env.PATH)&#10;// &#39;/usr/bin:/bin:/usr/sbin:/sbin:/usr/local/bin&#39;&#10;&#10;process.env.PATH.split(path.delimiter)&#10;// returns&#10;[&#39;/usr/bin&#39;, &#39;/bin&#39;, &#39;/usr/sbin&#39;, &#39;/sbin&#39;, &#39;/usr/local/bin&#39;]</code></pre>&#10;<p>An example on Windows:&#10;&#10;</p>&#10;<pre><code>console.log(process.env.PATH)&#10;// &#39;C:\Windows\system32;C:\Windows;C:\Program Files\nodejs\&#39;&#10;&#10;process.env.PATH.split(path.delimiter)&#10;// returns&#10;[&#39;C:\Windows\system32&#39;, &#39;C:\Windows&#39;, &#39;C:\Program Files\nodejs\&#39;]</code></pre>&#10;</field>
                this.delimiter = undefined;
            };
            case "net": return new     function net() {
                /// <summary><p>The <code>net</code> module provides you with an asynchronous network wrapper. It contains&#10;methods for creating both servers and clients (called streams). You can include&#10;this module with <code>require(&#39;net&#39;);</code>&#10;&#10;</p>&#10;</summary>
                this.createServer = function(options, connectionListener) {
                    /// <summary><p>Creates a new TCP server. The <code>connectionListener</code> argument is&#10;automatically set as a listener for the [&#39;connection&#39;][] event.&#10;&#10;</p>&#10;<p><code>options</code> is an object with the following defaults:&#10;&#10;</p>&#10;<pre><code>{ allowHalfOpen: false&#10;}</code></pre>&#10;<p>If <code>allowHalfOpen</code> is <code>true</code>, then the socket won&#39;t automatically send a FIN&#10;packet when the other end of the socket sends a FIN packet. The socket becomes&#10;non-readable, but still writable. You should call the <code>end()</code> method explicitly.&#10;See [&#39;end&#39;][] event for more information.&#10;&#10;</p>&#10;<p>Here is an example of an echo server which listens for connections&#10;on port 8124:&#10;&#10;</p>&#10;<pre><code>var net = require(&#39;net&#39;);&#10;var server = net.createServer(function(c) { //&#39;connection&#39; listener&#10;  console.log(&#39;server connected&#39;);&#10;  c.on(&#39;end&#39;, function() {&#10;    console.log(&#39;server disconnected&#39;);&#10;  });&#10;  c.write(&#39;hello\r\n&#39;);&#10;  c.pipe(c);&#10;});&#10;server.listen(8124, function() { //&#39;listening&#39; listener&#10;  console.log(&#39;server bound&#39;);&#10;});</code></pre>&#10;<p>Test this by using <code>telnet</code>:&#10;&#10;</p>&#10;<pre><code>telnet localhost 8124</code></pre>&#10;<p>To listen on the socket <code>/tmp/echo.sock</code> the third line from the last would&#10;just be changed to&#10;&#10;</p>&#10;<pre><code>server.listen(&#39;/tmp/echo.sock&#39;, function() { //&#39;listening&#39; listener</code></pre>&#10;<p>Use <code>nc</code> to connect to a UNIX domain socket server:&#10;&#10;</p>&#10;<pre><code>nc -U /tmp/echo.sock</code></pre>&#10;</summary>
                    /// <signature>
                    /// <param name="options"></param>
                    /// <param name="connectionListener"></param>
                    /// </signature>
                    return new this.Server();
                }
                this.connect = function(options, connectionListener) {
                    /// <summary><p>Constructs a new socket object and opens the socket to the given location.&#10;When the socket is established, the [&#39;connect&#39;][] event will be emitted.&#10;&#10;</p>&#10;<p>For TCP sockets, <code>options</code> argument should be an object which specifies:&#10;&#10;</p>&#10;<ul>&#10;<li><p><code>port</code>: Port the client should connect to (Required).</p>&#10;</li>&#10;<li><p><code>host</code>: Host the client should connect to. Defaults to <code>&#39;localhost&#39;</code>.</p>&#10;</li>&#10;<li><p><code>localAddress</code>: Local interface to bind to for network connections.</p>&#10;</li>&#10;</ul>&#10;<p>For UNIX domain sockets, <code>options</code> argument should be an object which specifies:&#10;&#10;</p>&#10;<ul>&#10;<li><code>path</code>: Path the client should connect to (Required).</li>&#10;</ul>&#10;<p>Common options are:&#10;&#10;</p>&#10;<ul>&#10;<li><code>allowHalfOpen</code>: if <code>true</code>, the socket won&#39;t automatically send&#10;a FIN packet when the other end of the socket sends a FIN packet.&#10;Defaults to <code>false</code>.  See [&#39;end&#39;][] event for more information.</li>&#10;</ul>&#10;<p>The <code>connectListener</code> parameter will be added as an listener for the&#10;[&#39;connect&#39;][] event.&#10;&#10;</p>&#10;<p>Here is an example of a client of echo server as described previously:&#10;&#10;</p>&#10;<pre><code>var net = require(&#39;net&#39;);&#10;var client = net.connect({port: 8124},&#10;    function() { //&#39;connect&#39; listener&#10;  console.log(&#39;client connected&#39;);&#10;  client.write(&#39;world!\r\n&#39;);&#10;});&#10;client.on(&#39;data&#39;, function(data) {&#10;  console.log(data.toString());&#10;  client.end();&#10;});&#10;client.on(&#39;end&#39;, function() {&#10;  console.log(&#39;client disconnected&#39;);&#10;});</code></pre>&#10;<p>To connect on the socket <code>/tmp/echo.sock</code> the second line would just be&#10;changed to&#10;&#10;</p>&#10;<pre><code>var client = net.connect({path: &#39;/tmp/echo.sock&#39;},</code></pre>&#10;</summary>
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
                    /// <summary><p>Constructs a new socket object and opens the socket to the given location.&#10;When the socket is established, the [&#39;connect&#39;][] event will be emitted.&#10;&#10;</p>&#10;<p>For TCP sockets, <code>options</code> argument should be an object which specifies:&#10;&#10;</p>&#10;<ul>&#10;<li><p><code>port</code>: Port the client should connect to (Required).</p>&#10;</li>&#10;<li><p><code>host</code>: Host the client should connect to. Defaults to <code>&#39;localhost&#39;</code>.</p>&#10;</li>&#10;<li><p><code>localAddress</code>: Local interface to bind to for network connections.</p>&#10;</li>&#10;</ul>&#10;<p>For UNIX domain sockets, <code>options</code> argument should be an object which specifies:&#10;&#10;</p>&#10;<ul>&#10;<li><code>path</code>: Path the client should connect to (Required).</li>&#10;</ul>&#10;<p>Common options are:&#10;&#10;</p>&#10;<ul>&#10;<li><code>allowHalfOpen</code>: if <code>true</code>, the socket won&#39;t automatically send&#10;a FIN packet when the other end of the socket sends a FIN packet.&#10;Defaults to <code>false</code>.  See [&#39;end&#39;][] event for more information.</li>&#10;</ul>&#10;<p>The <code>connectListener</code> parameter will be added as an listener for the&#10;[&#39;connect&#39;][] event.&#10;&#10;</p>&#10;<p>Here is an example of a client of echo server as described previously:&#10;&#10;</p>&#10;<pre><code>var net = require(&#39;net&#39;);&#10;var client = net.connect({port: 8124},&#10;    function() { //&#39;connect&#39; listener&#10;  console.log(&#39;client connected&#39;);&#10;  client.write(&#39;world!\r\n&#39;);&#10;});&#10;client.on(&#39;data&#39;, function(data) {&#10;  console.log(data.toString());&#10;  client.end();&#10;});&#10;client.on(&#39;end&#39;, function() {&#10;  console.log(&#39;client disconnected&#39;);&#10;});</code></pre>&#10;<p>To connect on the socket <code>/tmp/echo.sock</code> the second line would just be&#10;changed to&#10;&#10;</p>&#10;<pre><code>var client = net.connect({path: &#39;/tmp/echo.sock&#39;},</code></pre>&#10;</summary>
                    /// <signature>
                    /// <param name="options"></param>
                    /// <param name="connectionListener"></param>
                    /// </signature>
                    return new this.Connection();
                }
                this.connect = function(port, host, connectListener) {
                    /// <summary><p>Creates a TCP connection to <code>port</code> on <code>host</code>. If <code>host</code> is omitted,&#10;<code>&#39;localhost&#39;</code> will be assumed.&#10;The <code>connectListener</code> parameter will be added as an listener for the&#10;[&#39;connect&#39;][] event.&#10;&#10;</p>&#10;</summary>
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
                    /// <summary><p>Creates a TCP connection to <code>port</code> on <code>host</code>. If <code>host</code> is omitted,&#10;<code>&#39;localhost&#39;</code> will be assumed.&#10;The <code>connectListener</code> parameter will be added as an listener for the&#10;[&#39;connect&#39;][] event.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="port"></param>
                    /// <param name="host"></param>
                    /// <param name="connectListener"></param>
                    /// </signature>
                    return new this.Connection();
                }
                this.connect = function(path, connectListener) {
                    /// <summary><p>Creates unix socket connection to <code>path</code>.&#10;The <code>connectListener</code> parameter will be added as an listener for the&#10;[&#39;connect&#39;][] event.&#10;&#10;</p>&#10;</summary>
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
                    /// <summary><p>Creates unix socket connection to <code>path</code>.&#10;The <code>connectListener</code> parameter will be added as an listener for the&#10;[&#39;connect&#39;][] event.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="path"></param>
                    /// <param name="connectListener"></param>
                    /// </signature>
                    return new this.Connection();
                }
                this.isIP = function(input) {
                    /// <summary><p>Tests if input is an IP address. Returns 0 for invalid strings,&#10;returns 4 for IP version 4 addresses, and returns 6 for IP version 6 addresses.&#10;&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="input"></param>
                    /// </signature>
                }
                this.isIPv4 = function(input) {
                    /// <summary><p>Returns true if input is a version 4 IP address, otherwise returns false.&#10;&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="input"></param>
                    /// </signature>
                }
                this.isIPv6 = function(input) {
                    /// <summary><p>Returns true if input is a version 6 IP address, otherwise returns false.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="input"></param>
                    /// </signature>
                }
                function _Server() {
                    this.listen = function(port, host, backlog, callback) {
                        /// <summary><p>Begin accepting connections on the specified <code>port</code> and <code>host</code>.  If the&#10;<code>host</code> is omitted, the server will accept connections directed to any&#10;IPv4 address (<code>INADDR_ANY</code>). A port value of zero will assign a random port.&#10;&#10;</p>&#10;<p>Backlog is the maximum length of the queue of pending connections.&#10;The actual length will be determined by your OS through sysctl settings such as&#10;<code>tcp_max_syn_backlog</code> and <code>somaxconn</code> on linux. The default value of this&#10;parameter is 511 (not 512).&#10;&#10;</p>&#10;<p>This function is asynchronous.  When the server has been bound,&#10;[&#39;listening&#39;][] event will be emitted.  The last parameter <code>callback</code>&#10;will be added as an listener for the [&#39;listening&#39;][] event.&#10;&#10;</p>&#10;<p>One issue some users run into is getting <code>EADDRINUSE</code> errors. This means that&#10;another server is already running on the requested port. One way of handling this&#10;would be to wait a second and then try again. This can be done with&#10;&#10;</p>&#10;<pre><code>server.on(&#39;error&#39;, function (e) {&#10;  if (e.code == &#39;EADDRINUSE&#39;) {&#10;    console.log(&#39;Address in use, retrying...&#39;);&#10;    setTimeout(function () {&#10;      server.close();&#10;      server.listen(PORT, HOST);&#10;    }, 1000);&#10;  }&#10;});</code></pre>&#10;<p>(Note: All sockets in Node set <code>SO_REUSEADDR</code> already)&#10;&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="port"></param>
                        /// <param name="host"></param>
                        /// <param name="backlog"></param>
                        /// <param name="callback"></param>
                        /// </signature>
                    }
                    this.listen = function(path, callback) {
                        /// <summary><p>Start a UNIX socket server listening for connections on the given <code>path</code>.&#10;&#10;</p>&#10;<p>This function is asynchronous.  When the server has been bound,&#10;[&#39;listening&#39;][] event will be emitted.  The last parameter <code>callback</code>&#10;will be added as an listener for the [&#39;listening&#39;][] event.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="path"></param>
                        /// <param name="callback"></param>
                        /// </signature>
                    }
                    this.listen = function(handle, callback) {
                        /// <summary><p>The <code>handle</code> object can be set to either a server or socket (anything&#10;with an underlying <code>_handle</code> member), or a <code>{fd: &lt;n&gt;}</code> object.&#10;&#10;</p>&#10;<p>This will cause the server to accept connections on the specified&#10;handle, but it is presumed that the file descriptor or handle has&#10;already been bound to a port or domain socket.&#10;&#10;</p>&#10;<p>Listening on a file descriptor is not supported on Windows.&#10;&#10;</p>&#10;<p>This function is asynchronous.  When the server has been bound,&#10;<a href="#event_listening_">&#39;listening&#39;</a> event will be emitted.&#10;the last parameter <code>callback</code> will be added as an listener for the&#10;<a href="#event_listening_">&#39;listening&#39;</a> event.&#10;&#10;</p>&#10;</summary>
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
                        /// <summary><p>Stops the server from accepting new connections and keeps existing&#10;connections. This function is asynchronous, the server is finally&#10;closed when all connections are ended and the server emits a <code>&#39;close&#39;</code>&#10;event. Optionally, you can pass a callback to listen for the <code>&#39;close&#39;</code>&#10;event.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="callback"></param>
                        /// </signature>
                    }
                    this.address = function() {
                        /// <summary><p>Returns the bound address, the address family name and port of the server&#10;as reported by the operating system.&#10;Useful to find which port was assigned when giving getting an OS-assigned address.&#10;Returns an object with three properties, e.g.&#10;<code>{ port: 12346, family: &#39;IPv4&#39;, address: &#39;127.0.0.1&#39; }</code>&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>var server = net.createServer(function (socket) {&#10;  socket.end(&quot;goodbye\n&quot;);&#10;});&#10;&#10;// grab a random port.&#10;server.listen(function() {&#10;  address = server.address();&#10;  console.log(&quot;opened server on %j&quot;, address);&#10;});</code></pre>&#10;<p>Don&#39;t call <code>server.address()</code> until the <code>&#39;listening&#39;</code> event has been emitted.&#10;&#10;</p>&#10;</summary>
                    }
                    this.unref = function() {
                        /// <summary><p>Calling <code>unref</code> on a server will allow the program to exit if this is the only&#10;active server in the event system. If the server is already <code>unref</code>d calling&#10;<code>unref</code> again will have no effect.&#10;&#10;</p>&#10;</summary>
                    }
                    this.ref = function() {
                        /// <summary><p>Opposite of <code>unref</code>, calling <code>ref</code> on a previously <code>unref</code>d server will <em>not</em>&#10;let the program exit if it&#39;s the only server left (the default behavior). If&#10;the server is <code>ref</code>d calling <code>ref</code> again will have no effect.&#10;&#10;</p>&#10;</summary>
                    }
                    this.getConnections = function(callback) {
                        /// <summary><p>Asynchronously get the number of concurrent connections on the server. Works&#10;when sockets were sent to forks.&#10;&#10;</p>&#10;<p>Callback should take two arguments <code>err</code> and <code>count</code>.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="callback"></param>
                        /// </signature>
                    }
                    this.addListener = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// listening: Emitted when the server has been bound after calling <code>server.listen</code>. ...&#10;
                        /// connection: Emitted when a new connection is made. <code>socket</code> is an instance of ...&#10;
                        /// close: Emitted when the server closes. Note that if connections exist, this ...&#10;
                        /// error: Emitted when an error occurs.  The <code>&#39;close&#39;</code> event will be called directly ...&#10;
                        /// </summary>

                    }
                    this.once = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// listening: Emitted when the server has been bound after calling <code>server.listen</code>. ...&#10;
                        /// connection: Emitted when a new connection is made. <code>socket</code> is an instance of ...&#10;
                        /// close: Emitted when the server closes. Note that if connections exist, this ...&#10;
                        /// error: Emitted when an error occurs.  The <code>&#39;close&#39;</code> event will be called directly ...&#10;
                        /// </summary>

                    }
                    this.removeListener = function(event, listener) {
                        /// <summary>Supported Events: listening, connection, close, error</summary>
                    }
                    this.removeAllListeners = function(event) {
                        /// <summary>Supported Events: listening, connection, close, error</summary>
                    }
                    this.setMaxListeners = function(n) { }
                    this.listeners = function(event) {
                        /// <summary>Supported Events: listening, connection, close, error</summary>
                    }
                    this.emit = function(event, arguments) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// listening: Emitted when the server has been bound after calling <code>server.listen</code>. ...&#10;
                        /// connection: Emitted when a new connection is made. <code>socket</code> is an instance of ...&#10;
                        /// close: Emitted when the server closes. Note that if connections exist, this ...&#10;
                        /// error: Emitted when an error occurs.  The <code>&#39;close&#39;</code> event will be called directly ...&#10;
                        /// </summary>

                    }
                    this.on = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// listening: Emitted when the server has been bound after calling <code>server.listen</code>. ...&#10;
                        /// connection: Emitted when a new connection is made. <code>socket</code> is an instance of ...&#10;
                        /// close: Emitted when the server closes. Note that if connections exist, this ...&#10;
                        /// error: Emitted when an error occurs.  The <code>&#39;close&#39;</code> event will be called directly ...&#10;
                        /// </summary>

                    }
                    /// <field name='maxConnections'><p>Set this property to reject connections when the server&#39;s connection count gets&#10;high.&#10;&#10;</p>&#10;<p>It is not recommended to use this option once a socket has been sent to a child&#10;with <code>child_process.fork()</code>.&#10;&#10;</p>&#10;</field>
                    this.maxConnections = undefined;
                    /// <field name='connections'><p>This function is <strong>deprecated</strong>; please use [server.getConnections()][] instead.&#10;The number of concurrent connections on the server.&#10;&#10;</p>&#10;<p>This becomes <code>null</code> when sending a socket to a child with&#10;<code>child_process.fork()</code>. To poll forks and get current number of active&#10;connections use asynchronous <code>server.getConnections</code> instead.&#10;&#10;</p>&#10;<p><code>net.Server</code> is an [EventEmitter][] with the following events:&#10;&#10;</p>&#10;</field>
                    this.connections = undefined;
                }

                this.Server = function() {
                    return new _Server();
                }
                function _Socket() {
                    this.Socket = function(options) {
                        /// <summary><p>Construct a new socket object.&#10;&#10;</p>&#10;<p><code>options</code> is an object with the following defaults:&#10;&#10;</p>&#10;<pre><code>{ fd: null&#10;  type: null&#10;  allowHalfOpen: false&#10;}</code></pre>&#10;<p><code>fd</code> allows you to specify the existing file descriptor of socket. <code>type</code>&#10;specified underlying protocol. It can be <code>&#39;tcp4&#39;</code>, <code>&#39;tcp6&#39;</code>, or <code>&#39;unix&#39;</code>.&#10;About <code>allowHalfOpen</code>, refer to <code>createServer()</code> and <code>&#39;end&#39;</code> event.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="options"></param>
                        /// </signature>
                    }
                    this.connect = function(path, connectListener) {
                        /// <summary><p>Opens the connection for a given socket. If <code>port</code> and <code>host</code> are given,&#10;then the socket will be opened as a TCP socket, if <code>host</code> is omitted,&#10;<code>localhost</code> will be assumed. If a <code>path</code> is given, the socket will be&#10;opened as a unix socket to that path.&#10;&#10;</p>&#10;<p>Normally this method is not needed, as <code>net.createConnection</code> opens the&#10;socket. Use this only if you are implementing a custom Socket.&#10;&#10;</p>&#10;<p>This function is asynchronous. When the [&#39;connect&#39;][] event is emitted the&#10;socket is established. If there is a problem connecting, the <code>&#39;connect&#39;</code> event&#10;will not be emitted, the <code>&#39;error&#39;</code> event will be emitted with the exception.&#10;&#10;</p>&#10;<p>The <code>connectListener</code> parameter will be added as an listener for the&#10;[&#39;connect&#39;][] event.&#10;&#10;&#10;</p>&#10;</summary>
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
                        /// <summary><p>Opens the connection for a given socket. If <code>port</code> and <code>host</code> are given,&#10;then the socket will be opened as a TCP socket, if <code>host</code> is omitted,&#10;<code>localhost</code> will be assumed. If a <code>path</code> is given, the socket will be&#10;opened as a unix socket to that path.&#10;&#10;</p>&#10;<p>Normally this method is not needed, as <code>net.createConnection</code> opens the&#10;socket. Use this only if you are implementing a custom Socket.&#10;&#10;</p>&#10;<p>This function is asynchronous. When the [&#39;connect&#39;][] event is emitted the&#10;socket is established. If there is a problem connecting, the <code>&#39;connect&#39;</code> event&#10;will not be emitted, the <code>&#39;error&#39;</code> event will be emitted with the exception.&#10;&#10;</p>&#10;<p>The <code>connectListener</code> parameter will be added as an listener for the&#10;[&#39;connect&#39;][] event.&#10;&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="path"></param>
                        /// <param name="connectListener"></param>
                        /// </signature>
                    }
                    this.setEncoding = function(encoding) {
                        /// <summary><p>Set the encoding for the socket as a Readable Stream. See&#10;[stream.setEncoding()][] for more information.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="encoding"></param>
                        /// </signature>
                    }
                    this.write = function(data, encoding, callback) {
                        /// <summary><p>Sends data on the socket. The second parameter specifies the encoding in the&#10;case of a string--it defaults to UTF8 encoding.&#10;&#10;</p>&#10;<p>Returns <code>true</code> if the entire data was flushed successfully to the kernel&#10;buffer. Returns <code>false</code> if all or part of the data was queued in user memory.&#10;<code>&#39;drain&#39;</code> will be emitted when the buffer is again free.&#10;&#10;</p>&#10;<p>The optional <code>callback</code> parameter will be executed when the data is finally&#10;written out - this may not be immediately.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="data"></param>
                        /// <param name="encoding"></param>
                        /// <param name="callback"></param>
                        /// </signature>
                    }
                    this.end = function(data, encoding) {
                        /// <summary><p>Half-closes the socket. i.e., it sends a FIN packet. It is possible the&#10;server will still send some data.&#10;&#10;</p>&#10;<p>If <code>data</code> is specified, it is equivalent to calling&#10;<code>socket.write(data, encoding)</code> followed by <code>socket.end()</code>.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="data"></param>
                        /// <param name="encoding"></param>
                        /// </signature>
                    }
                    this.destroy = function() {
                        /// <summary><p>Ensures that no more I/O activity happens on this socket. Only necessary in&#10;case of errors (parse error or so).&#10;&#10;</p>&#10;</summary>
                    }
                    this.pause = function() {
                        /// <summary><p>Pauses the reading of data. That is, <code>&#39;data&#39;</code> events will not be emitted.&#10;Useful to throttle back an upload.&#10;&#10;</p>&#10;</summary>
                    }
                    this.resume = function() {
                        /// <summary><p>Resumes reading after a call to <code>pause()</code>.&#10;&#10;</p>&#10;</summary>
                    }
                    this.setTimeout = function(timeout, callback) {
                        /// <summary><p>Sets the socket to timeout after <code>timeout</code> milliseconds of inactivity on&#10;the socket. By default <code>net.Socket</code> do not have a timeout.&#10;&#10;</p>&#10;<p>When an idle timeout is triggered the socket will receive a <code>&#39;timeout&#39;</code>&#10;event but the connection will not be severed. The user must manually <code>end()</code>&#10;or <code>destroy()</code> the socket.&#10;&#10;</p>&#10;<p>If <code>timeout</code> is 0, then the existing idle timeout is disabled.&#10;&#10;</p>&#10;<p>The optional <code>callback</code> parameter will be added as a one time listener for the&#10;<code>&#39;timeout&#39;</code> event.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="timeout"></param>
                        /// <param name="callback"></param>
                        /// </signature>
                    }
                    this.setNoDelay = function(noDelay) {
                        /// <summary><p>Disables the Nagle algorithm. By default TCP connections use the Nagle&#10;algorithm, they buffer data before sending it off. Setting <code>true</code> for&#10;<code>noDelay</code> will immediately fire off data each time <code>socket.write()</code> is called.&#10;<code>noDelay</code> defaults to <code>true</code>.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="noDelay"></param>
                        /// </signature>
                    }
                    this.setKeepAlive = function(enable, initialDelay) {
                        /// <summary><p>Enable/disable keep-alive functionality, and optionally set the initial&#10;delay before the first keepalive probe is sent on an idle socket.&#10;<code>enable</code> defaults to <code>false</code>.&#10;&#10;</p>&#10;<p>Set <code>initialDelay</code> (in milliseconds) to set the delay between the last&#10;data packet received and the first keepalive probe. Setting 0 for&#10;initialDelay will leave the value unchanged from the default&#10;(or previous) setting. Defaults to <code>0</code>.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="enable"></param>
                        /// <param name="initialDelay"></param>
                        /// </signature>
                    }
                    this.address = function() {
                        /// <summary><p>Returns the bound address, the address family name and port of the&#10;socket as reported by the operating system. Returns an object with&#10;three properties, e.g.&#10;<code>{ port: 12346, family: &#39;IPv4&#39;, address: &#39;127.0.0.1&#39; }</code>&#10;&#10;</p>&#10;</summary>
                    }
                    this.unref = function() {
                        /// <summary><p>Calling <code>unref</code> on a socket will allow the program to exit if this is the only&#10;active socket in the event system. If the socket is already <code>unref</code>d calling&#10;<code>unref</code> again will have no effect.&#10;&#10;</p>&#10;</summary>
                    }
                    this.ref = function() {
                        /// <summary><p>Opposite of <code>unref</code>, calling <code>ref</code> on a previously <code>unref</code>d socket will <em>not</em>&#10;let the program exit if it&#39;s the only socket left (the default behavior). If&#10;the socket is <code>ref</code>d calling <code>ref</code> again will have no effect.&#10;&#10;</p>&#10;</summary>
                    }
                    this.addListener = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// connect: Emitted when a socket connection is successfully established. ...&#10;
                        /// data: Emitted when data is received.  The argument <code>data</code> will be a <code>Buffer</code> or ...&#10;
                        /// end: Emitted when the other end of the socket sends a FIN packet. ...&#10;
                        /// timeout: Emitted if the socket times out from inactivity. This is only to notify that ...&#10;
                        /// drain: Emitted when the write buffer becomes empty. Can be used to throttle uploads. ...&#10;
                        /// error: Emitted when an error occurs.  The <code>&#39;close&#39;</code> event will be called directly ...&#10;
                        /// close: Emitted once the socket is fully closed. The argument <code>had_error</code> is a boolean ...&#10;
                        /// </summary>

                    }
                    this.once = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// connect: Emitted when a socket connection is successfully established. ...&#10;
                        /// data: Emitted when data is received.  The argument <code>data</code> will be a <code>Buffer</code> or ...&#10;
                        /// end: Emitted when the other end of the socket sends a FIN packet. ...&#10;
                        /// timeout: Emitted if the socket times out from inactivity. This is only to notify that ...&#10;
                        /// drain: Emitted when the write buffer becomes empty. Can be used to throttle uploads. ...&#10;
                        /// error: Emitted when an error occurs.  The <code>&#39;close&#39;</code> event will be called directly ...&#10;
                        /// close: Emitted once the socket is fully closed. The argument <code>had_error</code> is a boolean ...&#10;
                        /// </summary>

                    }
                    this.removeListener = function(event, listener) {
                        /// <summary>Supported Events: connect, data, end, timeout, drain, error, close</summary>
                    }
                    this.removeAllListeners = function(event) {
                        /// <summary>Supported Events: connect, data, end, timeout, drain, error, close</summary>
                    }
                    this.setMaxListeners = function(n) { }
                    this.listeners = function(event) {
                        /// <summary>Supported Events: connect, data, end, timeout, drain, error, close</summary>
                    }
                    this.emit = function(event, arguments) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// connect: Emitted when a socket connection is successfully established. ...&#10;
                        /// data: Emitted when data is received.  The argument <code>data</code> will be a <code>Buffer</code> or ...&#10;
                        /// end: Emitted when the other end of the socket sends a FIN packet. ...&#10;
                        /// timeout: Emitted if the socket times out from inactivity. This is only to notify that ...&#10;
                        /// drain: Emitted when the write buffer becomes empty. Can be used to throttle uploads. ...&#10;
                        /// error: Emitted when an error occurs.  The <code>&#39;close&#39;</code> event will be called directly ...&#10;
                        /// close: Emitted once the socket is fully closed. The argument <code>had_error</code> is a boolean ...&#10;
                        /// </summary>

                    }
                    this.on = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// connect: Emitted when a socket connection is successfully established. ...&#10;
                        /// data: Emitted when data is received.  The argument <code>data</code> will be a <code>Buffer</code> or ...&#10;
                        /// end: Emitted when the other end of the socket sends a FIN packet. ...&#10;
                        /// timeout: Emitted if the socket times out from inactivity. This is only to notify that ...&#10;
                        /// drain: Emitted when the write buffer becomes empty. Can be used to throttle uploads. ...&#10;
                        /// error: Emitted when an error occurs.  The <code>&#39;close&#39;</code> event will be called directly ...&#10;
                        /// close: Emitted once the socket is fully closed. The argument <code>had_error</code> is a boolean ...&#10;
                        /// </summary>

                    }
                    /// <field name='bufferSize'><p><code>net.Socket</code> has the property that <code>socket.write()</code> always works. This is to&#10;help users get up and running quickly. The computer cannot always keep up&#10;with the amount of data that is written to a socket - the network connection&#10;simply might be too slow. Node will internally queue up the data written to a&#10;socket and send it out over the wire when it is possible. (Internally it is&#10;polling on the socket&#39;s file descriptor for being writable).&#10;&#10;</p>&#10;<p>The consequence of this internal buffering is that memory may grow. This&#10;property shows the number of characters currently buffered to be written.&#10;(Number of characters is approximately equal to the number of bytes to be&#10;written, but the buffer may contain strings, and the strings are lazily&#10;encoded, so the exact number of bytes is not known.)&#10;&#10;</p>&#10;<p>Users who experience large or growing <code>bufferSize</code> should attempt to&#10;&quot;throttle&quot; the data flows in their program with <code>pause()</code> and <code>resume()</code>.&#10;&#10;&#10;</p>&#10;</field>
                    this.bufferSize = undefined;
                    /// <field name='remoteAddress'><p>The string representation of the remote IP address. For example,&#10;<code>&#39;74.125.127.100&#39;</code> or <code>&#39;2001:4860:a005::68&#39;</code>.&#10;&#10;</p>&#10;</field>
                    this.remoteAddress = undefined;
                    /// <field name='remotePort'><p>The numeric representation of the remote port. For example,&#10;<code>80</code> or <code>21</code>.&#10;&#10;</p>&#10;</field>
                    this.remotePort = undefined;
                    /// <field name='localAddress'><p>The string representation of the local IP address the remote client is&#10;connecting on. For example, if you are listening on <code>&#39;0.0.0.0&#39;</code> and the&#10;client connects on <code>&#39;192.168.1.1&#39;</code>, the value would be <code>&#39;192.168.1.1&#39;</code>.&#10;&#10;</p>&#10;</field>
                    this.localAddress = undefined;
                    /// <field name='localPort'><p>The numeric representation of the local port. For example,&#10;<code>80</code> or <code>21</code>.&#10;&#10;</p>&#10;</field>
                    this.localPort = undefined;
                    /// <field name='bytesRead'><p>The amount of received bytes.&#10;&#10;</p>&#10;</field>
                    this.bytesRead = undefined;
                    /// <field name='bytesWritten'><p>The amount of bytes sent.&#10;&#10;&#10;</p>&#10;<p><code>net.Socket</code> instances are [EventEmitter][] with the following events:&#10;&#10;</p>&#10;</field>
                    this.bytesWritten = undefined;
                }

                this.Socket = function() {
                    return new _Socket();
                }
            };
            case "dgram": return new     function dgram() {
                /// <summary><p>Datagram sockets are available through <code>require(&#39;dgram&#39;)</code>.&#10;&#10;</p>&#10;<p>Important note: the behavior of <code>dgram.Socket#bind()</code> has changed in v0.10&#10;and is always asynchronous now.  If you have code that looks like this:&#10;&#10;</p>&#10;<pre><code>var s = dgram.createSocket(&#39;udp4&#39;);&#10;s.bind(1234);&#10;s.addMembership(&#39;224.0.0.114&#39;);</code></pre>&#10;<p>You have to change it to this:&#10;&#10;</p>&#10;<pre><code>var s = dgram.createSocket(&#39;udp4&#39;);&#10;s.bind(1234, function() {&#10;  s.addMembership(&#39;224.0.0.114&#39;);&#10;});</code></pre>&#10;</summary>
                this.createSocket = function(type, callback) {
                    /// <summary><p>Creates a datagram Socket of the specified types.  Valid types are <code>udp4</code>&#10;and <code>udp6</code>.&#10;&#10;</p>&#10;<p>Takes an optional callback which is added as a listener for <code>message</code> events.&#10;&#10;</p>&#10;<p>Call <code>socket.bind</code> if you want to receive datagrams. <code>socket.bind()</code> will bind&#10;to the &quot;all interfaces&quot; address on a random port (it does the right thing for&#10;both <code>udp4</code> and <code>udp6</code> sockets). You can then retrieve the address and port&#10;with <code>socket.address().address</code> and <code>socket.address().port</code>.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="type">String. Either 'udp4' or 'udp6'</param>
                    /// <param name="callback">Function. Attached as a listener to `message` events.</param>
                    /// <returns>Socket object</returns>
                    /// </signature>
                    /// <signature>
                    /// <param name="type"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                    return new this.Socket();
                }
                function _Socket() {
                    this.send = function(buf, offset, length, port, address, callback) {
                        /// <summary><p>For UDP sockets, the destination port and IP address must be specified.  A string&#10;may be supplied for the <code>address</code> parameter, and it will be resolved with DNS.  An&#10;optional callback may be specified to detect any DNS errors and when <code>buf</code> may be&#10;re-used.  Note that DNS lookups will delay the time that a send takes place, at&#10;least until the next tick.  The only way to know for sure that a send has taken place&#10;is to use the callback.&#10;&#10;</p>&#10;<p>If the socket has not been previously bound with a call to <code>bind</code>, it&#39;s&#10;assigned a random port number and bound to the &quot;all interfaces&quot; address&#10;(0.0.0.0 for <code>udp4</code> sockets, ::0 for <code>udp6</code> sockets).&#10;&#10;</p>&#10;<p>Example of sending a UDP packet to a random port on <code>localhost</code>;&#10;&#10;</p>&#10;<pre><code>var dgram = require(&#39;dgram&#39;);&#10;var message = new Buffer(&quot;Some bytes&quot;);&#10;var client = dgram.createSocket(&quot;udp4&quot;);&#10;client.send(message, 0, message.length, 41234, &quot;localhost&quot;, function(err, bytes) {&#10;  client.close();&#10;});</code></pre>&#10;<p><strong>A Note about UDP datagram size</strong>&#10;&#10;</p>&#10;<p>The maximum size of an <code>IPv4/v6</code> datagram depends on the <code>MTU</code> (<em>Maximum Transmission Unit</em>)&#10;and on the <code>Payload Length</code> field size.&#10;&#10;</p>&#10;<ul>&#10;<li><p>The <code>Payload Length</code> field is <code>16 bits</code> wide, which means that a normal payload&#10;cannot be larger than 64K octets including internet header and data&#10;(65,507 bytes = 65,535 − 8 bytes UDP header − 20 bytes IP header);&#10;this is generally true for loopback interfaces, but such long datagrams&#10;are impractical for most hosts and networks.</p>&#10;</li>&#10;<li><p>The <code>MTU</code> is the largest size a given link layer technology can support for datagrams.&#10;For any link, <code>IPv4</code> mandates a minimum <code>MTU</code> of <code>68</code> octets, while the recommended <code>MTU</code>&#10;for IPv4 is <code>576</code> (typically recommended as the <code>MTU</code> for dial-up type applications),&#10;whether they arrive whole or in fragments.</p>&#10;<p>For <code>IPv6</code>, the minimum <code>MTU</code> is <code>1280</code> octets, however, the mandatory minimum&#10;fragment reassembly buffer size is <code>1500</code> octets.&#10;The value of <code>68</code> octets is very small, since most current link layer technologies have&#10;a minimum <code>MTU</code> of <code>1500</code> (like Ethernet).</p>&#10;</li>&#10;</ul>&#10;<p>Note that it&#39;s impossible to know in advance the MTU of each link through which&#10;a packet might travel, and that generally sending a datagram greater than&#10;the (receiver) <code>MTU</code> won&#39;t work (the packet gets silently dropped, without&#10;informing the source that the data did not reach its intended recipient).&#10;&#10;</p>&#10;</summary>
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
                    this.bind = function(port, address, callback) {
                        /// <summary><p>For UDP sockets, listen for datagrams on a named <code>port</code> and optional&#10;<code>address</code>. If <code>address</code> is not specified, the OS will try to listen on&#10;all addresses.  After binding is done, a &quot;listening&quot; event is emitted&#10;and the <code>callback</code>(if specified) is called. Specifying both a&#10;&quot;listening&quot; event listener and <code>callback</code> is not harmful but not very&#10;useful.&#10;&#10;</p>&#10;<p>A bound datagram socket keeps the node process running to receive&#10;datagrams.&#10;&#10;</p>&#10;<p>If binding fails, an &quot;error&quot; event is generated. In rare case (e.g.&#10;binding a closed socket), an <code>Error</code> may be thrown by this method.&#10;&#10;</p>&#10;<p>Example of a UDP server listening on port 41234:&#10;&#10;</p>&#10;<pre><code>var dgram = require(&quot;dgram&quot;);&#10;&#10;var server = dgram.createSocket(&quot;udp4&quot;);&#10;&#10;server.on(&quot;error&quot;, function (err) {&#10;  console.log(&quot;server error:\n&quot; + err.stack);&#10;  server.close();&#10;});&#10;&#10;server.on(&quot;message&quot;, function (msg, rinfo) {&#10;  console.log(&quot;server got: &quot; + msg + &quot; from &quot; +&#10;    rinfo.address + &quot;:&quot; + rinfo.port);&#10;});&#10;&#10;server.on(&quot;listening&quot;, function () {&#10;  var address = server.address();&#10;  console.log(&quot;server listening &quot; +&#10;      address.address + &quot;:&quot; + address.port);&#10;});&#10;&#10;server.bind(41234);&#10;// server listening 0.0.0.0:41234</code></pre>&#10;</summary>
                        /// <signature>
                        /// <param name="port">Integer</param>
                        /// <param name="address">String</param>
                        /// <param name="callback">Function with no parameters, Optional. Callback when binding is done.</param>
                        /// </signature>
                        /// <signature>
                        /// <param name="port"></param>
                        /// <param name="address"></param>
                        /// <param name="callback"></param>
                        /// </signature>
                    }
                    this.close = function() {
                        /// <summary><p>Close the underlying socket and stop listening for data on it.&#10;&#10;</p>&#10;</summary>
                    }
                    this.address = function() {
                        /// <summary><p>Returns an object containing the address information for a socket.  For UDP sockets,&#10;this object will contain <code>address</code> , <code>family</code> and <code>port</code>.&#10;&#10;</p>&#10;</summary>
                    }
                    this.setBroadcast = function(flag) {
                        /// <summary><p>Sets or clears the <code>SO_BROADCAST</code> socket option.  When this option is set, UDP packets&#10;may be sent to a local interface&#39;s broadcast address.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="flag">Boolean</param>
                        /// </signature>
                        /// <signature>
                        /// <param name="flag"></param>
                        /// </signature>
                    }
                    this.setTTL = function(ttl) {
                        /// <summary><p>Sets the <code>IP_TTL</code> socket option.  TTL stands for &quot;Time to Live,&quot; but in this context it&#10;specifies the number of IP hops that a packet is allowed to go through.  Each router or&#10;gateway that forwards a packet decrements the TTL.  If the TTL is decremented to 0 by a&#10;router, it will not be forwarded.  Changing TTL values is typically done for network&#10;probes or when multicasting.&#10;&#10;</p>&#10;<p>The argument to <code>setTTL()</code> is a number of hops between 1 and 255.  The default on most&#10;systems is 64.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="ttl">Integer</param>
                        /// </signature>
                        /// <signature>
                        /// <param name="ttl"></param>
                        /// </signature>
                    }
                    this.setMulticastTTL = function(ttl) {
                        /// <summary><p>Sets the <code>IP_MULTICAST_TTL</code> socket option.  TTL stands for &quot;Time to Live,&quot; but in this&#10;context it specifies the number of IP hops that a packet is allowed to go through,&#10;specifically for multicast traffic.  Each router or gateway that forwards a packet&#10;decrements the TTL. If the TTL is decremented to 0 by a router, it will not be forwarded.&#10;&#10;</p>&#10;<p>The argument to <code>setMulticastTTL()</code> is a number of hops between 0 and 255.  The default on most&#10;systems is 1.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="ttl">Integer</param>
                        /// </signature>
                        /// <signature>
                        /// <param name="ttl"></param>
                        /// </signature>
                    }
                    this.setMulticastLoopback = function(flag) {
                        /// <summary><p>Sets or clears the <code>IP_MULTICAST_LOOP</code> socket option.  When this option is set, multicast&#10;packets will also be received on the local interface.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="flag">Boolean</param>
                        /// </signature>
                        /// <signature>
                        /// <param name="flag"></param>
                        /// </signature>
                    }
                    this.addMembership = function(multicastAddress, multicastInterface) {
                        /// <summary><p>Tells the kernel to join a multicast group with <code>IP_ADD_MEMBERSHIP</code> socket option.&#10;&#10;</p>&#10;<p>If <code>multicastInterface</code> is not specified, the OS will try to add membership to all valid&#10;interfaces.&#10;&#10;</p>&#10;</summary>
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
                        /// <summary><p>Opposite of <code>addMembership</code> - tells the kernel to leave a multicast group with&#10;<code>IP_DROP_MEMBERSHIP</code> socket option. This is automatically called by the kernel&#10;when the socket is closed or process terminates, so most apps will never need to call&#10;this.&#10;&#10;</p>&#10;<p>If <code>multicastInterface</code> is not specified, the OS will try to drop membership to all valid&#10;interfaces.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="multicastAddress">String</param>
                        /// <param name="multicastInterface">String</param>
                        /// </signature>
                        /// <signature>
                        /// <param name="multicastAddress"></param>
                        /// <param name="multicastInterface"></param>
                        /// </signature>
                    }
                    this.unref = function() {
                        /// <summary><p>Calling <code>unref</code> on a socket will allow the program to exit if this is the only&#10;active socket in the event system. If the socket is already <code>unref</code>d calling&#10;<code>unref</code> again will have no effect.&#10;&#10;</p>&#10;</summary>
                    }
                    this.ref = function() {
                        /// <summary><p>Opposite of <code>unref</code>, calling <code>ref</code> on a previously <code>unref</code>d socket will <em>not</em>&#10;let the program exit if it&#39;s the only socket left (the default behavior). If&#10;the socket is <code>ref</code>d calling <code>ref</code> again will have no effect.&#10;&#10;</p>&#10;</summary>
                    }
                    this.addListener = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// message: Emitted when a new datagram is available on a socket.  <code>msg</code> is a <code>Buffer</code> and <code>rinfo</code> is ...&#10;
                        /// listening: Emitted when a socket starts listening for datagrams.  This happens as soon as UDP sockets ...&#10;
                        /// close: Emitted when a socket is closed with <code>close()</code>.  No new <code>message</code> events will be emitted ...&#10;
                        /// error: Emitted when an error occurs. ...&#10;
                        /// </summary>

                    }
                    this.once = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// message: Emitted when a new datagram is available on a socket.  <code>msg</code> is a <code>Buffer</code> and <code>rinfo</code> is ...&#10;
                        /// listening: Emitted when a socket starts listening for datagrams.  This happens as soon as UDP sockets ...&#10;
                        /// close: Emitted when a socket is closed with <code>close()</code>.  No new <code>message</code> events will be emitted ...&#10;
                        /// error: Emitted when an error occurs. ...&#10;
                        /// </summary>

                    }
                    this.removeListener = function(event, listener) {
                        /// <summary>Supported Events: message, listening, close, error</summary>
                    }
                    this.removeAllListeners = function(event) {
                        /// <summary>Supported Events: message, listening, close, error</summary>
                    }
                    this.setMaxListeners = function(n) { }
                    this.listeners = function(event) {
                        /// <summary>Supported Events: message, listening, close, error</summary>
                    }
                    this.emit = function(event, arguments) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// message: Emitted when a new datagram is available on a socket.  <code>msg</code> is a <code>Buffer</code> and <code>rinfo</code> is ...&#10;
                        /// listening: Emitted when a socket starts listening for datagrams.  This happens as soon as UDP sockets ...&#10;
                        /// close: Emitted when a socket is closed with <code>close()</code>.  No new <code>message</code> events will be emitted ...&#10;
                        /// error: Emitted when an error occurs. ...&#10;
                        /// </summary>

                    }
                    this.on = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// message: Emitted when a new datagram is available on a socket.  <code>msg</code> is a <code>Buffer</code> and <code>rinfo</code> is ...&#10;
                        /// listening: Emitted when a socket starts listening for datagrams.  This happens as soon as UDP sockets ...&#10;
                        /// close: Emitted when a socket is closed with <code>close()</code>.  No new <code>message</code> events will be emitted ...&#10;
                        /// error: Emitted when an error occurs. ...&#10;
                        /// </summary>

                    }
                }

                this.Socket = function() {
                    return new _Socket();
                }
            };
            case "dns": return new     function dns() {
                /// <summary><p>Use <code>require(&#39;dns&#39;)</code> to access this module. All methods in the dns module&#10;use C-Ares except for <code>dns.lookup</code> which uses <code>getaddrinfo(3)</code> in a thread&#10;pool. C-Ares is much faster than <code>getaddrinfo</code> but the system resolver is&#10;more constant with how other programs operate. When a user does&#10;<code>net.connect(80, &#39;google.com&#39;)</code> or <code>http.get({ host: &#39;google.com&#39; })</code> the&#10;<code>dns.lookup</code> method is used. Users who need to do a large number of lookups&#10;quickly should use the methods that go through C-Ares.&#10;&#10;</p>&#10;<p>Here is an example which resolves <code>&#39;www.google.com&#39;</code> then reverse&#10;resolves the IP addresses which are returned.&#10;&#10;</p>&#10;<pre><code>var dns = require(&#39;dns&#39;);&#10;&#10;dns.resolve4(&#39;www.google.com&#39;, function (err, addresses) {&#10;  if (err) throw err;&#10;&#10;  console.log(&#39;addresses: &#39; + JSON.stringify(addresses));&#10;&#10;  addresses.forEach(function (a) {&#10;    dns.reverse(a, function (err, domains) {&#10;      if (err) {&#10;        throw err;&#10;      }&#10;&#10;      console.log(&#39;reverse for &#39; + a + &#39;: &#39; + JSON.stringify(domains));&#10;    });&#10;  });&#10;});</code></pre>&#10;</summary>
                this.lookup = function(domain, family, callback) {
                    /// <summary><p>Resolves a domain (e.g. <code>&#39;google.com&#39;</code>) into the first found A (IPv4) or&#10;AAAA (IPv6) record.&#10;The <code>family</code> can be the integer <code>4</code> or <code>6</code>. Defaults to <code>null</code> that indicates&#10;both Ip v4 and v6 address family.&#10;&#10;</p>&#10;<p>The callback has arguments <code>(err, address, family)</code>.  The <code>address</code> argument&#10;is a string representation of a IP v4 or v6 address. The <code>family</code> argument&#10;is either the integer 4 or 6 and denotes the family of <code>address</code> (not&#10;necessarily the value initially passed to <code>lookup</code>).&#10;&#10;</p>&#10;<p>On error, <code>err</code> is an <code>Error</code> object, where <code>err.code</code> is the error code.&#10;Keep in mind that <code>err.code</code> will be set to <code>&#39;ENOENT&#39;</code> not only when&#10;the domain does not exist but also when the lookup fails in other ways&#10;such as no available file descriptors.&#10;&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="domain"></param>
                    /// <param name="family"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.resolve = function(domain, rrtype, callback) {
                    /// <summary><p>Resolves a domain (e.g. <code>&#39;google.com&#39;</code>) into an array of the record types&#10;specified by rrtype. Valid rrtypes are <code>&#39;A&#39;</code> (IPV4 addresses, default),&#10;<code>&#39;AAAA&#39;</code> (IPV6 addresses), <code>&#39;MX&#39;</code> (mail exchange records), <code>&#39;TXT&#39;</code> (text&#10;records), <code>&#39;SRV&#39;</code> (SRV records), <code>&#39;PTR&#39;</code> (used for reverse IP lookups),&#10;<code>&#39;NS&#39;</code> (name server records) and <code>&#39;CNAME&#39;</code> (canonical name records).&#10;&#10;</p>&#10;<p>The callback has arguments <code>(err, addresses)</code>.  The type of each item&#10;in <code>addresses</code> is determined by the record type, and described in the&#10;documentation for the corresponding lookup methods below.&#10;&#10;</p>&#10;<p>On error, <code>err</code> is an <code>Error</code> object, where <code>err.code</code> is&#10;one of the error codes listed below.&#10;&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="domain"></param>
                    /// <param name="rrtype"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.resolve4 = function(domain, callback) {
                    /// <summary><p>The same as <code>dns.resolve()</code>, but only for IPv4 queries (<code>A</code> records).&#10;<code>addresses</code> is an array of IPv4 addresses (e.g.&#10;<code>[&#39;74.125.79.104&#39;, &#39;74.125.79.105&#39;, &#39;74.125.79.106&#39;]</code>).&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="domain"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.resolve6 = function(domain, callback) {
                    /// <summary><p>The same as <code>dns.resolve4()</code> except for IPv6 queries (an <code>AAAA</code> query).&#10;&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="domain"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.resolveMx = function(domain, callback) {
                    /// <summary><p>The same as <code>dns.resolve()</code>, but only for mail exchange queries (<code>MX</code> records).&#10;&#10;</p>&#10;<p><code>addresses</code> is an array of MX records, each with a priority and an exchange&#10;attribute (e.g. <code>[{&#39;priority&#39;: 10, &#39;exchange&#39;: &#39;mx.example.com&#39;},...]</code>).&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="domain"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.resolveTxt = function(domain, callback) {
                    /// <summary><p>The same as <code>dns.resolve()</code>, but only for text queries (<code>TXT</code> records).&#10;<code>addresses</code> is an array of the text records available for <code>domain</code> (e.g.,&#10;<code>[&#39;v=spf1 ip4:0.0.0.0 ~all&#39;]</code>).&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="domain"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.resolveSrv = function(domain, callback) {
                    /// <summary><p>The same as <code>dns.resolve()</code>, but only for service records (<code>SRV</code> records).&#10;<code>addresses</code> is an array of the SRV records available for <code>domain</code>. Properties&#10;of SRV records are priority, weight, port, and name (e.g.,&#10;<code>[{&#39;priority&#39;: 10, {&#39;weight&#39;: 5, &#39;port&#39;: 21223, &#39;name&#39;: &#39;service.example.com&#39;}, ...]</code>).&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="domain"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.resolveNs = function(domain, callback) {
                    /// <summary><p>The same as <code>dns.resolve()</code>, but only for name server records (<code>NS</code> records).&#10;<code>addresses</code> is an array of the name server records available for <code>domain</code>&#10;(e.g., <code>[&#39;ns1.example.com&#39;, &#39;ns2.example.com&#39;]</code>).&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="domain"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.resolveCname = function(domain, callback) {
                    /// <summary><p>The same as <code>dns.resolve()</code>, but only for canonical name records (<code>CNAME</code>&#10;records). <code>addresses</code> is an array of the canonical name records available for&#10;<code>domain</code> (e.g., <code>[&#39;bar.example.com&#39;]</code>).&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="domain"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.reverse = function(ip, callback) {
                    /// <summary><p>Reverse resolves an ip address to an array of domain names.&#10;&#10;</p>&#10;<p>The callback has arguments <code>(err, domains)</code>.&#10;&#10;</p>&#10;<p>On error, <code>err</code> is an <code>Error</code> object, where <code>err.code</code> is&#10;one of the error codes listed below.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="ip"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
            };
            case "http": return new     function http() {
                /// <summary><p>To use the HTTP server and client one must <code>require(&#39;http&#39;)</code>.&#10;&#10;</p>&#10;<p>The HTTP interfaces in Node are designed to support many features&#10;of the protocol which have been traditionally difficult to use.&#10;In particular, large, possibly chunk-encoded, messages. The interface is&#10;careful to never buffer entire requests or responses--the&#10;user is able to stream data.&#10;&#10;</p>&#10;<p>HTTP message headers are represented by an object like this:&#10;&#10;</p>&#10;<pre><code>{ &#39;content-length&#39;: &#39;123&#39;,&#10;  &#39;content-type&#39;: &#39;text/plain&#39;,&#10;  &#39;connection&#39;: &#39;keep-alive&#39;,&#10;  &#39;accept&#39;: &#39;*/*&#39; }</code></pre>&#10;<p>Keys are lowercased. Values are not modified.&#10;&#10;</p>&#10;<p>In order to support the full spectrum of possible HTTP applications, Node&#39;s&#10;HTTP API is very low-level. It deals with stream handling and message&#10;parsing only. It parses a message into headers and body but it does not&#10;parse the actual headers or the body.&#10;&#10;&#10;</p>&#10;</summary>
                this.createServer = function(requestListener) {
                    /// <summary><p>Returns a new web server object.&#10;&#10;</p>&#10;<p>The <code>requestListener</code> is a function which is automatically&#10;added to the <code>&#39;request&#39;</code> event.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="requestListener"></param>
                    /// </signature>
                    return new this.Server();
                }
                this.createClient = function(port, host) {
                    /// <summary><p>This function is <strong>deprecated</strong>; please use [http.request()][] instead.&#10;Constructs a new HTTP client. <code>port</code> and <code>host</code> refer to the server to be&#10;connected to.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="port"></param>
                    /// <param name="host"></param>
                    /// </signature>
                    return new this.Client();
                }
                this.request = function(options, callback) {
                    /// <summary><p>Node maintains several connections per server to make HTTP requests.&#10;This function allows one to transparently issue requests.&#10;&#10;</p>&#10;<p><code>options</code> can be an object or a string. If <code>options</code> is a string, it is&#10;automatically parsed with [url.parse()][].&#10;&#10;</p>&#10;<p>Options:&#10;&#10;</p>&#10;<ul>&#10;<li><code>host</code>: A domain name or IP address of the server to issue the request to.&#10;Defaults to <code>&#39;localhost&#39;</code>.</li>&#10;<li><code>hostname</code>: To support <code>url.parse()</code> <code>hostname</code> is preferred over <code>host</code></li>&#10;<li><code>port</code>: Port of remote server. Defaults to 80.</li>&#10;<li><code>localAddress</code>: Local interface to bind for network connections.</li>&#10;<li><code>socketPath</code>: Unix Domain Socket (use one of host:port or socketPath)</li>&#10;<li><code>method</code>: A string specifying the HTTP request method. Defaults to <code>&#39;GET&#39;</code>.</li>&#10;<li><code>path</code>: Request path. Defaults to <code>&#39;/&#39;</code>. Should include query string if any.&#10;E.G. <code>&#39;/index.html?page=12&#39;</code></li>&#10;<li><code>headers</code>: An object containing request headers.</li>&#10;<li><code>auth</code>: Basic authentication i.e. <code>&#39;user:password&#39;</code> to compute an&#10;Authorization header.</li>&#10;<li><code>agent</code>: Controls [Agent][] behavior. When an Agent is used request will&#10;default to <code>Connection: keep-alive</code>. Possible values:<ul>&#10;<li><code>undefined</code> (default): use [global Agent][] for this host and port.</li>&#10;<li><code>Agent</code> object: explicitly use the passed in <code>Agent</code>.</li>&#10;<li><code>false</code>: opts out of connection pooling with an Agent, defaults request to&#10;<code>Connection: close</code>.</li>&#10;</ul>&#10;</li>&#10;</ul>&#10;<p><code>http.request()</code> returns an instance of the <code>http.ClientRequest</code>&#10;class. The <code>ClientRequest</code> instance is a writable stream. If one needs to&#10;upload a file with a POST request, then write to the <code>ClientRequest</code> object.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>var options = {&#10;  hostname: &#39;www.google.com&#39;,&#10;  port: 80,&#10;  path: &#39;/upload&#39;,&#10;  method: &#39;POST&#39;&#10;};&#10;&#10;var req = http.request(options, function(res) {&#10;  console.log(&#39;STATUS: &#39; + res.statusCode);&#10;  console.log(&#39;HEADERS: &#39; + JSON.stringify(res.headers));&#10;  res.setEncoding(&#39;utf8&#39;);&#10;  res.on(&#39;data&#39;, function (chunk) {&#10;    console.log(&#39;BODY: &#39; + chunk);&#10;  });&#10;});&#10;&#10;req.on(&#39;error&#39;, function(e) {&#10;  console.log(&#39;problem with request: &#39; + e.message);&#10;});&#10;&#10;// write data to request body&#10;req.write(&#39;data\n&#39;);&#10;req.write(&#39;data\n&#39;);&#10;req.end();</code></pre>&#10;<p>Note that in the example <code>req.end()</code> was called. With <code>http.request()</code> one&#10;must always call <code>req.end()</code> to signify that you&#39;re done with the request -&#10;even if there is no data being written to the request body.&#10;&#10;</p>&#10;<p>If any error is encountered during the request (be that with DNS resolution,&#10;TCP level errors, or actual HTTP parse errors) an <code>&#39;error&#39;</code> event is emitted&#10;on the returned request object.&#10;&#10;</p>&#10;<p>There are a few special headers that should be noted.&#10;&#10;</p>&#10;<ul>&#10;<li><p>Sending a &#39;Connection: keep-alive&#39; will notify Node that the connection to&#10;the server should be persisted until the next request.</p>&#10;</li>&#10;<li><p>Sending a &#39;Content-length&#39; header will disable the default chunked encoding.</p>&#10;</li>&#10;<li><p>Sending an &#39;Expect&#39; header will immediately send the request headers.&#10;Usually, when sending &#39;Expect: 100-continue&#39;, you should both set a timeout&#10;and listen for the <code>continue</code> event. See RFC2616 Section 8.2.3 for more&#10;information.</p>&#10;</li>&#10;<li><p>Sending an Authorization header will override using the <code>auth</code> option&#10;to compute basic authentication.</p>&#10;</li>&#10;</ul>&#10;</summary>
                    /// <signature>
                    /// <param name="options"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.get = function(options, callback) {
                    /// <summary><p>Since most requests are GET requests without bodies, Node provides this&#10;convenience method. The only difference between this method and <code>http.request()</code>&#10;is that it sets the method to GET and calls <code>req.end()</code> automatically.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>http.get(&quot;http://www.google.com/index.html&quot;, function(res) {&#10;  console.log(&quot;Got response: &quot; + res.statusCode);&#10;}).on(&#39;error&#39;, function(e) {&#10;  console.log(&quot;Got error: &quot; + e.message);&#10;});</code></pre>&#10;</summary>
                    /// <signature>
                    /// <param name="options"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                function _Server() {
                    this.listen = function(port, hostname, backlog, callback) {
                        /// <summary><p>Begin accepting connections on the specified port and hostname.  If the&#10;hostname is omitted, the server will accept connections directed to any&#10;IPv4 address (<code>INADDR_ANY</code>).&#10;&#10;</p>&#10;<p>To listen to a unix socket, supply a filename instead of port and hostname.&#10;&#10;</p>&#10;<p>Backlog is the maximum length of the queue of pending connections.&#10;The actual length will be determined by your OS through sysctl settings such as&#10;<code>tcp_max_syn_backlog</code> and <code>somaxconn</code> on linux. The default value of this&#10;parameter is 511 (not 512).&#10;&#10;</p>&#10;<p>This function is asynchronous. The last parameter <code>callback</code> will be added as&#10;a listener for the [&#39;listening&#39;][] event.  See also [net.Server.listen(port)][].&#10;&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="port"></param>
                        /// <param name="hostname"></param>
                        /// <param name="backlog"></param>
                        /// <param name="callback"></param>
                        /// </signature>
                    }
                    this.listen = function(path, callback) {
                        /// <summary><p>Start a UNIX socket server listening for connections on the given <code>path</code>.&#10;&#10;</p>&#10;<p>This function is asynchronous. The last parameter <code>callback</code> will be added as&#10;a listener for the [&#39;listening&#39;][] event.  See also [net.Server.listen(path)][].&#10;&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="path"></param>
                        /// <param name="callback"></param>
                        /// </signature>
                    }
                    this.listen = function(handle, callback) {
                        /// <summary><p>The <code>handle</code> object can be set to either a server or socket (anything&#10;with an underlying <code>_handle</code> member), or a <code>{fd: &lt;n&gt;}</code> object.&#10;&#10;</p>&#10;<p>This will cause the server to accept connections on the specified&#10;handle, but it is presumed that the file descriptor or handle has&#10;already been bound to a port or domain socket.&#10;&#10;</p>&#10;<p>Listening on a file descriptor is not supported on Windows.&#10;&#10;</p>&#10;<p>This function is asynchronous. The last parameter <code>callback</code> will be added as&#10;a listener for the <a href="net.html#event_listening_">&#39;listening&#39;</a> event.&#10;See also <a href="net.html#net_server_listen_handle_callback">net.Server.listen()</a>.&#10;&#10;</p>&#10;</summary>
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
                        /// <summary><p>Stops the server from accepting new connections.  See [net.Server.close()][].&#10;&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="callback"></param>
                        /// </signature>
                    }
                    this.setTimeout = function(msecs, callback) {
                        /// <summary><p>Sets the timeout value for sockets, and emits a <code>&#39;timeout&#39;</code> event on&#10;the Server object, passing the socket as an argument, if a timeout&#10;occurs.&#10;&#10;</p>&#10;<p>If there is a <code>&#39;timeout&#39;</code> event listener on the Server object, then it&#10;will be called with the timed-out socket as an argument.&#10;&#10;</p>&#10;<p>By default, the Server&#39;s timeout value is 2 minutes, and sockets are&#10;destroyed automatically if they time out.  However, if you assign a&#10;callback to the Server&#39;s <code>&#39;timeout&#39;</code> event, then you are responsible&#10;for handling socket timeouts.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="msecs" type="Number"></param>
                        /// <param name="callback" type="Function"></param>
                        /// </signature>
                        /// <signature>
                        /// <param name="msecs"></param>
                        /// <param name="callback"></param>
                        /// </signature>
                    }
                    this.addListener = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// request: <code>function (request, response) { }</code> ...&#10;
                        /// connection: <code>function (socket) { }</code> ...&#10;
                        /// close: <code>function () { }</code> ...&#10;
                        /// checkContinue: <code>function (request, response) { }</code> ...&#10;
                        /// connect: <code>function (request, socket, head) { }</code> ...&#10;
                        /// upgrade: <code>function (request, socket, head) { }</code> ...&#10;
                        /// clientError: <code>function (exception, socket) { }</code> ...&#10;
                        /// </summary>

                    }
                    this.once = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// request: <code>function (request, response) { }</code> ...&#10;
                        /// connection: <code>function (socket) { }</code> ...&#10;
                        /// close: <code>function () { }</code> ...&#10;
                        /// checkContinue: <code>function (request, response) { }</code> ...&#10;
                        /// connect: <code>function (request, socket, head) { }</code> ...&#10;
                        /// upgrade: <code>function (request, socket, head) { }</code> ...&#10;
                        /// clientError: <code>function (exception, socket) { }</code> ...&#10;
                        /// </summary>

                    }
                    this.removeListener = function(event, listener) {
                        /// <summary>Supported Events: request, connection, close, checkContinue, connect, upgrade, clientError</summary>
                    }
                    this.removeAllListeners = function(event) {
                        /// <summary>Supported Events: request, connection, close, checkContinue, connect, upgrade, clientError</summary>
                    }
                    this.setMaxListeners = function(n) { }
                    this.listeners = function(event) {
                        /// <summary>Supported Events: request, connection, close, checkContinue, connect, upgrade, clientError</summary>
                    }
                    this.emit = function(event, arguments) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// request: <code>function (request, response) { }</code> ...&#10;
                        /// connection: <code>function (socket) { }</code> ...&#10;
                        /// close: <code>function () { }</code> ...&#10;
                        /// checkContinue: <code>function (request, response) { }</code> ...&#10;
                        /// connect: <code>function (request, socket, head) { }</code> ...&#10;
                        /// upgrade: <code>function (request, socket, head) { }</code> ...&#10;
                        /// clientError: <code>function (exception, socket) { }</code> ...&#10;
                        /// </summary>

                    }
                    this.on = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// request: <code>function (request, response) { }</code> ...&#10;
                        /// connection: <code>function (socket) { }</code> ...&#10;
                        /// close: <code>function () { }</code> ...&#10;
                        /// checkContinue: <code>function (request, response) { }</code> ...&#10;
                        /// connect: <code>function (request, socket, head) { }</code> ...&#10;
                        /// upgrade: <code>function (request, socket, head) { }</code> ...&#10;
                        /// clientError: <code>function (exception, socket) { }</code> ...&#10;
                        /// </summary>

                    }
                    /// <field name='maxHeadersCount'><p>Limits maximum incoming headers count, equal to 1000 by default. If set to 0 -&#10;no limit will be applied.&#10;&#10;</p>&#10;</field>
                    this.maxHeadersCount = undefined;
                    /// <field name='timeout'><p>The number of milliseconds of inactivity before a socket is presumed&#10;to have timed out.&#10;&#10;</p>&#10;<p>Note that the socket timeout logic is set up on connection, so&#10;changing this value only affects <em>new</em> connections to the server, not&#10;any existing connections.&#10;&#10;</p>&#10;<p>Set to 0 to disable any kind of automatic timeout behavior on incoming&#10;connections.&#10;&#10;</p>&#10;</field>
                    this.timeout = undefined;
                }

                this.Server = function() {
                    return new _Server();
                }
                function _ServerResponse() {
                    this.writeContinue = function() {
                        /// <summary><p>Sends a HTTP/1.1 100 Continue message to the client, indicating that&#10;the request body should be sent. See the [&#39;checkContinue&#39;][] event on <code>Server</code>.&#10;&#10;</p>&#10;</summary>
                    }
                    this.writeHead = function(statusCode, reasonPhrase, headers) {
                        /// <summary><p>Sends a response header to the request. The status code is a 3-digit HTTP&#10;status code, like <code>404</code>. The last argument, <code>headers</code>, are the response headers.&#10;Optionally one can give a human-readable <code>reasonPhrase</code> as the second&#10;argument.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>var body = &#39;hello world&#39;;&#10;response.writeHead(200, {&#10;  &#39;Content-Length&#39;: body.length,&#10;  &#39;Content-Type&#39;: &#39;text/plain&#39; });</code></pre>&#10;<p>This method must only be called once on a message and it must&#10;be called before <code>response.end()</code> is called.&#10;&#10;</p>&#10;<p>If you call <code>response.write()</code> or <code>response.end()</code> before calling this, the&#10;implicit/mutable headers will be calculated and call this function for you.&#10;&#10;</p>&#10;<p>Note: that Content-Length is given in bytes not characters. The above example&#10;works because the string <code>&#39;hello world&#39;</code> contains only single byte characters.&#10;If the body contains higher coded characters then <code>Buffer.byteLength()</code>&#10;should be used to determine the number of bytes in a given encoding.&#10;And Node does not check whether Content-Length and the length of the body&#10;which has been transmitted are equal or not.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="statusCode"></param>
                        /// <param name="reasonPhrase"></param>
                        /// <param name="headers"></param>
                        /// </signature>
                    }
                    this.setTimeout = function(msecs, callback) {
                        /// <summary><p>Sets the Socket&#39;s timeout value to <code>msecs</code>.  If a callback is&#10;provided, then it is added as a listener on the <code>&#39;timeout&#39;</code> event on&#10;the response object.&#10;&#10;</p>&#10;<p>If no <code>&#39;timeout&#39;</code> listener is added to the request, the response, or&#10;the server, then sockets are destroyed when they time out.  If you&#10;assign a handler on the request, the response, or the server&#39;s&#10;<code>&#39;timeout&#39;</code> events, then it is your responsibility to handle timed out&#10;sockets.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="msecs" type="Number"></param>
                        /// <param name="callback" type="Function"></param>
                        /// </signature>
                        /// <signature>
                        /// <param name="msecs"></param>
                        /// <param name="callback"></param>
                        /// </signature>
                    }
                    this.setHeader = function(name, value) {
                        /// <summary><p>Sets a single header value for implicit headers.  If this header already exists&#10;in the to-be-sent headers, its value will be replaced.  Use an array of strings&#10;here if you need to send multiple headers with the same name.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>response.setHeader(&quot;Content-Type&quot;, &quot;text/html&quot;);</code></pre>&#10;<p>or&#10;&#10;</p>&#10;<pre><code>response.setHeader(&quot;Set-Cookie&quot;, [&quot;type=ninja&quot;, &quot;language=javascript&quot;]);</code></pre>&#10;</summary>
                        /// <signature>
                        /// <param name="name"></param>
                        /// <param name="value"></param>
                        /// </signature>
                    }
                    this.getHeader = function(name) {
                        /// <summary><p>Reads out a header that&#39;s already been queued but not sent to the client.  Note&#10;that the name is case insensitive.  This can only be called before headers get&#10;implicitly flushed.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>var contentType = response.getHeader(&#39;content-type&#39;);</code></pre>&#10;</summary>
                        /// <signature>
                        /// <param name="name"></param>
                        /// </signature>
                    }
                    this.removeHeader = function(name) {
                        /// <summary><p>Removes a header that&#39;s queued for implicit sending.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>response.removeHeader(&quot;Content-Encoding&quot;);</code></pre>&#10;</summary>
                        /// <signature>
                        /// <param name="name"></param>
                        /// </signature>
                    }
                    this.write = function(chunk, encoding) {
                        /// <summary><p>If this method is called and <code>response.writeHead()</code> has not been called, it will&#10;switch to implicit header mode and flush the implicit headers.&#10;&#10;</p>&#10;<p>This sends a chunk of the response body. This method may&#10;be called multiple times to provide successive parts of the body.&#10;&#10;</p>&#10;<p><code>chunk</code> can be a string or a buffer. If <code>chunk</code> is a string,&#10;the second parameter specifies how to encode it into a byte stream.&#10;By default the <code>encoding</code> is <code>&#39;utf8&#39;</code>.&#10;&#10;</p>&#10;<p><strong>Note</strong>: This is the raw HTTP body and has nothing to do with&#10;higher-level multi-part body encodings that may be used.&#10;&#10;</p>&#10;<p>The first time <code>response.write()</code> is called, it will send the buffered&#10;header information and the first body to the client. The second time&#10;<code>response.write()</code> is called, Node assumes you&#39;re going to be streaming&#10;data, and sends that separately. That is, the response is buffered up to the&#10;first chunk of body.&#10;&#10;</p>&#10;<p>Returns <code>true</code> if the entire data was flushed successfully to the kernel&#10;buffer. Returns <code>false</code> if all or part of the data was queued in user memory.&#10;<code>&#39;drain&#39;</code> will be emitted when the buffer is again free.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="chunk"></param>
                        /// <param name="encoding"></param>
                        /// </signature>
                    }
                    this.addTrailers = function(headers) {
                        /// <summary><p>This method adds HTTP trailing headers (a header but at the end of the&#10;message) to the response.&#10;&#10;</p>&#10;<p>Trailers will <strong>only</strong> be emitted if chunked encoding is used for the&#10;response; if it is not (e.g., if the request was HTTP/1.0), they will&#10;be silently discarded.&#10;&#10;</p>&#10;<p>Note that HTTP requires the <code>Trailer</code> header to be sent if you intend to&#10;emit trailers, with a list of the header fields in its value. E.g.,&#10;&#10;</p>&#10;<pre><code>response.writeHead(200, { &#39;Content-Type&#39;: &#39;text/plain&#39;,&#10;                          &#39;Trailer&#39;: &#39;Content-MD5&#39; });&#10;response.write(fileData);&#10;response.addTrailers({&#39;Content-MD5&#39;: &quot;7895bf4b8828b55ceaf47747b4bca667&quot;});&#10;response.end();</code></pre>&#10;</summary>
                        /// <signature>
                        /// <param name="headers"></param>
                        /// </signature>
                    }
                    this.end = function(data, encoding) {
                        /// <summary><p>This method signals to the server that all of the response headers and body&#10;have been sent; that server should consider this message complete.&#10;The method, <code>response.end()</code>, MUST be called on each&#10;response.&#10;&#10;</p>&#10;<p>If <code>data</code> is specified, it is equivalent to calling <code>response.write(data, encoding)</code>&#10;followed by <code>response.end()</code>.&#10;&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="data"></param>
                        /// <param name="encoding"></param>
                        /// </signature>
                    }
                    this.addListener = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// close: <code>function () { }</code> ...&#10;
                        /// </summary>

                    }
                    this.once = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// close: <code>function () { }</code> ...&#10;
                        /// </summary>

                    }
                    this.removeListener = function(event, listener) {
                        /// <summary>Supported Events: close</summary>
                    }
                    this.removeAllListeners = function(event) {
                        /// <summary>Supported Events: close</summary>
                    }
                    this.setMaxListeners = function(n) { }
                    this.listeners = function(event) {
                        /// <summary>Supported Events: close</summary>
                    }
                    this.emit = function(event, arguments) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// close: <code>function () { }</code> ...&#10;
                        /// </summary>

                    }
                    this.on = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// close: <code>function () { }</code> ...&#10;
                        /// </summary>

                    }
                    /// <field name='statusCode'><p>When using implicit headers (not calling <code>response.writeHead()</code> explicitly), this property&#10;controls the status code that will be sent to the client when the headers get&#10;flushed.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>response.statusCode = 404;</code></pre>&#10;<p>After response header was sent to the client, this property indicates the&#10;status code which was sent out.&#10;&#10;</p>&#10;</field>
                    this.statusCode = undefined;
                    /// <field name='headersSent'><p>Boolean (read-only). True if headers were sent, false otherwise.&#10;&#10;</p>&#10;</field>
                    this.headersSent = undefined;
                    /// <field name='sendDate'><p>When true, the Date header will be automatically generated and sent in&#10;the response if it is not already present in the headers. Defaults to true.&#10;&#10;</p>&#10;<p>This should only be disabled for testing; HTTP requires the Date header&#10;in responses.&#10;&#10;</p>&#10;</field>
                    this.sendDate = undefined;
                }

                this.ServerResponse = function() {
                    return new _ServerResponse();
                }
                function _Agent() {
                    /// <field name='maxSockets'><p>By default set to 5. Determines how many concurrent sockets the agent can have&#10;open per host.&#10;&#10;</p>&#10;</field>
                    this.maxSockets = undefined;
                    /// <field name='sockets'><p>An object which contains arrays of sockets currently in use by the Agent. Do not&#10;modify.&#10;&#10;</p>&#10;</field>
                    this.sockets = undefined;
                    /// <field name='requests'><p>An object which contains queues of requests that have not yet been assigned to&#10;sockets. Do not modify.&#10;&#10;</p>&#10;</field>
                    this.requests = undefined;
                }

                this.Agent = function() {
                    return new _Agent();
                }
                function _ClientRequest() {
                    this.write = function(chunk, encoding) {
                        /// <summary><p>Sends a chunk of the body.  By calling this method&#10;many times, the user can stream a request body to a&#10;server--in that case it is suggested to use the&#10;<code>[&#39;Transfer-Encoding&#39;, &#39;chunked&#39;]</code> header line when&#10;creating the request.&#10;&#10;</p>&#10;<p>The <code>chunk</code> argument should be a [Buffer][] or a string.&#10;&#10;</p>&#10;<p>The <code>encoding</code> argument is optional and only applies when <code>chunk</code> is a string.&#10;Defaults to <code>&#39;utf8&#39;</code>.&#10;&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="chunk"></param>
                        /// <param name="encoding"></param>
                        /// </signature>
                    }
                    this.end = function(data, encoding) {
                        /// <summary><p>Finishes sending the request. If any parts of the body are&#10;unsent, it will flush them to the stream. If the request is&#10;chunked, this will send the terminating <code>&#39;0\r\n\r\n&#39;</code>.&#10;&#10;</p>&#10;<p>If <code>data</code> is specified, it is equivalent to calling&#10;<code>request.write(data, encoding)</code> followed by <code>request.end()</code>.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="data"></param>
                        /// <param name="encoding"></param>
                        /// </signature>
                    }
                    this.abort = function() {
                        /// <summary><p>Aborts a request.  (New since v0.3.8.)&#10;&#10;</p>&#10;</summary>
                    }
                    this.setTimeout = function(timeout, callback) {
                        /// <summary><p>Once a socket is assigned to this request and is connected&#10;[socket.setTimeout()][] will be called.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="timeout"></param>
                        /// <param name="callback"></param>
                        /// </signature>
                    }
                    this.setNoDelay = function(noDelay) {
                        /// <summary><p>Once a socket is assigned to this request and is connected&#10;[socket.setNoDelay()][] will be called.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="noDelay"></param>
                        /// </signature>
                    }
                    this.setSocketKeepAlive = function(enable, initialDelay) {
                        /// <summary><p>Once a socket is assigned to this request and is connected&#10;[socket.setKeepAlive()][] will be called.&#10;&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="enable"></param>
                        /// <param name="initialDelay"></param>
                        /// </signature>
                    }
                    this.addListener = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// response: <code>function (response) { }</code> ...&#10;
                        /// socket: <code>function (socket) { }</code> ...&#10;
                        /// connect: <code>function (response, socket, head) { }</code> ...&#10;
                        /// upgrade: <code>function (response, socket, head) { }</code> ...&#10;
                        /// continue: <code>function () { }</code> ...&#10;
                        /// </summary>

                    }
                    this.once = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// response: <code>function (response) { }</code> ...&#10;
                        /// socket: <code>function (socket) { }</code> ...&#10;
                        /// connect: <code>function (response, socket, head) { }</code> ...&#10;
                        /// upgrade: <code>function (response, socket, head) { }</code> ...&#10;
                        /// continue: <code>function () { }</code> ...&#10;
                        /// </summary>

                    }
                    this.removeListener = function(event, listener) {
                        /// <summary>Supported Events: response, socket, connect, upgrade, continue</summary>
                    }
                    this.removeAllListeners = function(event) {
                        /// <summary>Supported Events: response, socket, connect, upgrade, continue</summary>
                    }
                    this.setMaxListeners = function(n) { }
                    this.listeners = function(event) {
                        /// <summary>Supported Events: response, socket, connect, upgrade, continue</summary>
                    }
                    this.emit = function(event, arguments) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// response: <code>function (response) { }</code> ...&#10;
                        /// socket: <code>function (socket) { }</code> ...&#10;
                        /// connect: <code>function (response, socket, head) { }</code> ...&#10;
                        /// upgrade: <code>function (response, socket, head) { }</code> ...&#10;
                        /// continue: <code>function () { }</code> ...&#10;
                        /// </summary>

                    }
                    this.on = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// response: <code>function (response) { }</code> ...&#10;
                        /// socket: <code>function (socket) { }</code> ...&#10;
                        /// connect: <code>function (response, socket, head) { }</code> ...&#10;
                        /// upgrade: <code>function (response, socket, head) { }</code> ...&#10;
                        /// continue: <code>function () { }</code> ...&#10;
                        /// </summary>

                    }
                }

                this.ClientRequest = function() {
                    return new _ClientRequest();
                }
                /// <field name='STATUS_CODES'><p>A collection of all the standard HTTP response status codes, and the&#10;short description of each.  For example, <code>http.STATUS_CODES[404] === &#39;Not&#10;Found&#39;</code>.&#10;&#10;</p>&#10;</field>
                this.STATUS_CODES = undefined;
                /// <field name='globalAgent'><p>Global instance of Agent which is used as the default for all http client&#10;requests.&#10;&#10;&#10;</p>&#10;</field>
                this.globalAgent = undefined;
                /// <field name='IncomingMessage'><p>An <code>IncomingMessage</code> object is created by <code>http.Server</code> or <code>http.ClientRequest</code>&#10;and passed as the first argument to the <code>&#39;request&#39;</code> and <code>&#39;response&#39;</code> event&#10;respectively. It may be used to access response status, headers and data.&#10;&#10;</p>&#10;<p>It implements the [Readable Stream][] interface, as well as the&#10;following additional events, methods, and properties.&#10;&#10;</p>&#10;</field>
                this.IncomingMessage = undefined;
            };
            case "https": return new     function https() {
                /// <summary><p>HTTPS is the HTTP protocol over TLS/SSL. In Node this is implemented as a&#10;separate module.&#10;&#10;</p>&#10;</summary>
                this.createServer = function(options, requestListener) {
                    /// <summary><p>Returns a new HTTPS web server object. The <code>options</code> is similar to&#10;[tls.createServer()][].  The <code>requestListener</code> is a function which is&#10;automatically added to the <code>&#39;request&#39;</code> event.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>// curl -k https://localhost:8000/&#10;var https = require(&#39;https&#39;);&#10;var fs = require(&#39;fs&#39;);&#10;&#10;var options = {&#10;  key: fs.readFileSync(&#39;test/fixtures/keys/agent2-key.pem&#39;),&#10;  cert: fs.readFileSync(&#39;test/fixtures/keys/agent2-cert.pem&#39;)&#10;};&#10;&#10;https.createServer(options, function (req, res) {&#10;  res.writeHead(200);&#10;  res.end(&quot;hello world\n&quot;);&#10;}).listen(8000);</code></pre>&#10;<p>Or&#10;&#10;</p>&#10;<pre><code>var https = require(&#39;https&#39;);&#10;var fs = require(&#39;fs&#39;);&#10;&#10;var options = {&#10;  pfx: fs.readFileSync(&#39;server.pfx&#39;)&#10;};&#10;&#10;https.createServer(options, function (req, res) {&#10;  res.writeHead(200);&#10;  res.end(&quot;hello world\n&quot;);&#10;}).listen(8000);</code></pre>&#10;</summary>
                    /// <signature>
                    /// <param name="options"></param>
                    /// <param name="requestListener"></param>
                    /// </signature>
                    return new this.Server();
                }
                this.request = function(options, callback) {
                    /// <summary><p>Makes a request to a secure web server.&#10;&#10;</p>&#10;<p><code>options</code> can be an object or a string. If <code>options</code> is a string, it is&#10;automatically parsed with <a href="url.html#url.parse">url.parse()</a>.&#10;&#10;</p>&#10;<p>All options from [http.request()][] are valid.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>var https = require(&#39;https&#39;);&#10;&#10;var options = {&#10;  hostname: &#39;encrypted.google.com&#39;,&#10;  port: 443,&#10;  path: &#39;/&#39;,&#10;  method: &#39;GET&#39;&#10;};&#10;&#10;var req = https.request(options, function(res) {&#10;  console.log(&quot;statusCode: &quot;, res.statusCode);&#10;  console.log(&quot;headers: &quot;, res.headers);&#10;&#10;  res.on(&#39;data&#39;, function(d) {&#10;    process.stdout.write(d);&#10;  });&#10;});&#10;req.end();&#10;&#10;req.on(&#39;error&#39;, function(e) {&#10;  console.error(e);&#10;});</code></pre>&#10;<p>The options argument has the following options&#10;&#10;</p>&#10;<ul>&#10;<li><code>host</code>: A domain name or IP address of the server to issue the request to.&#10;Defaults to <code>&#39;localhost&#39;</code>.</li>&#10;<li><code>hostname</code>: To support <code>url.parse()</code> <code>hostname</code> is preferred over <code>host</code></li>&#10;<li><code>port</code>: Port of remote server. Defaults to 443.</li>&#10;<li><code>method</code>: A string specifying the HTTP request method. Defaults to <code>&#39;GET&#39;</code>.</li>&#10;<li><code>path</code>: Request path. Defaults to <code>&#39;/&#39;</code>. Should include query string if any.&#10;E.G. <code>&#39;/index.html?page=12&#39;</code></li>&#10;<li><code>headers</code>: An object containing request headers.</li>&#10;<li><code>auth</code>: Basic authentication i.e. <code>&#39;user:password&#39;</code> to compute an&#10;Authorization header.</li>&#10;<li><code>agent</code>: Controls [Agent][] behavior. When an Agent is used request will&#10;default to <code>Connection: keep-alive</code>. Possible values:<ul>&#10;<li><code>undefined</code> (default): use [globalAgent][] for this host and port.</li>&#10;<li><code>Agent</code> object: explicitly use the passed in <code>Agent</code>.</li>&#10;<li><code>false</code>: opts out of connection pooling with an Agent, defaults request to&#10;<code>Connection: close</code>.</li>&#10;</ul>&#10;</li>&#10;</ul>&#10;<p>The following options from [tls.connect()][] can also be specified. However, a&#10;[globalAgent][] silently ignores these.&#10;&#10;</p>&#10;<ul>&#10;<li><code>pfx</code>: Certificate, Private key and CA certificates to use for SSL. Default <code>null</code>.</li>&#10;<li><code>key</code>: Private key to use for SSL. Default <code>null</code>.</li>&#10;<li><code>passphrase</code>: A string of passphrase for the private key or pfx. Default <code>null</code>.</li>&#10;<li><code>cert</code>: Public x509 certificate to use. Default <code>null</code>.</li>&#10;<li><code>ca</code>: An authority certificate or array of authority certificates to check&#10;the remote host against.</li>&#10;<li><code>ciphers</code>: A string describing the ciphers to use or exclude. Consult&#10;<a href="http://www.openssl.org/docs/apps/ciphers.html#CIPHER_LIST_FORMAT">http://www.openssl.org/docs/apps/ciphers.html#CIPHER_LIST_FORMAT</a> for&#10;details on the format.</li>&#10;<li><code>rejectUnauthorized</code>: If <code>true</code>, the server certificate is verified against&#10;the list of supplied CAs. An <code>&#39;error&#39;</code> event is emitted if verification&#10;fails. Verification happens at the connection level, <em>before</em> the HTTP&#10;request is sent. Default <code>true</code>.</li>&#10;<li><code>secureProtocol</code>: The SSL method to use, e.g. <code>SSLv3_method</code> to force&#10;SSL version 3. The possible values depend on your installation of&#10;OpenSSL and are defined in the constant [SSL_METHODS][].</li>&#10;</ul>&#10;<p>In order to specify these options, use a custom <code>Agent</code>.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>var options = {&#10;  hostname: &#39;encrypted.google.com&#39;,&#10;  port: 443,&#10;  path: &#39;/&#39;,&#10;  method: &#39;GET&#39;,&#10;  key: fs.readFileSync(&#39;test/fixtures/keys/agent2-key.pem&#39;),&#10;  cert: fs.readFileSync(&#39;test/fixtures/keys/agent2-cert.pem&#39;)&#10;};&#10;options.agent = new https.Agent(options);&#10;&#10;var req = https.request(options, function(res) {&#10;  ...&#10;}</code></pre>&#10;<p>Or does not use an <code>Agent</code>.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>var options = {&#10;  hostname: &#39;encrypted.google.com&#39;,&#10;  port: 443,&#10;  path: &#39;/&#39;,&#10;  method: &#39;GET&#39;,&#10;  key: fs.readFileSync(&#39;test/fixtures/keys/agent2-key.pem&#39;),&#10;  cert: fs.readFileSync(&#39;test/fixtures/keys/agent2-cert.pem&#39;),&#10;  agent: false&#10;};&#10;&#10;var req = https.request(options, function(res) {&#10;  ...&#10;}</code></pre>&#10;</summary>
                    /// <signature>
                    /// <param name="options"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.get = function(options, callback) {
                    /// <summary><p>Like <code>http.get()</code> but for HTTPS.&#10;&#10;</p>&#10;<p><code>options</code> can be an object or a string. If <code>options</code> is a string, it is&#10;automatically parsed with <a href="url.html#url.parse">url.parse()</a>.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>var https = require(&#39;https&#39;);&#10;&#10;https.get(&#39;https://encrypted.google.com/&#39;, function(res) {&#10;  console.log(&quot;statusCode: &quot;, res.statusCode);&#10;  console.log(&quot;headers: &quot;, res.headers);&#10;&#10;  res.on(&#39;data&#39;, function(d) {&#10;    process.stdout.write(d);&#10;  });&#10;&#10;}).on(&#39;error&#39;, function(e) {&#10;  console.error(e);&#10;});</code></pre>&#10;</summary>
                    /// <signature>
                    /// <param name="options"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                function _Server() {
                }

                this.Server = function() {
                    return new _Server();
                }
                function _Agent() {
                }

                this.Agent = function() {
                    return new _Agent();
                }
                /// <field name='globalAgent'><p>Global instance of [https.Agent][] for all HTTPS client requests.&#10;&#10;</p>&#10;</field>
                this.globalAgent = undefined;
            };
            case "url": return new     function url() {
                /// <summary><p>This module has utilities for URL resolution and parsing.&#10;Call <code>require(&#39;url&#39;)</code> to use it.&#10;&#10;</p>&#10;<p>Parsed URL objects have some or all of the following fields, depending on&#10;whether or not they exist in the URL string. Any parts that are not in the URL&#10;string will not be in the parsed object. Examples are shown for the URL&#10;&#10;</p>&#10;<p><code>&#39;http://user:pass@host.com:8080/p/a/t/h?query=string#hash&#39;</code>&#10;&#10;</p>&#10;<ul>&#10;<li><p><code>href</code>: The full URL that was originally parsed. Both the protocol and host are lowercased.</p>&#10;<p>  Example: <code>&#39;http://user:pass@host.com:8080/p/a/t/h?query=string#hash&#39;</code></p>&#10;</li>&#10;<li><p><code>protocol</code>: The request protocol, lowercased.</p>&#10;<p>  Example: <code>&#39;http:&#39;</code></p>&#10;</li>&#10;<li><p><code>host</code>: The full lowercased host portion of the URL, including port&#10;information.</p>&#10;<p>  Example: <code>&#39;host.com:8080&#39;</code></p>&#10;</li>&#10;<li><p><code>auth</code>: The authentication information portion of a URL.</p>&#10;<p>  Example: <code>&#39;user:pass&#39;</code></p>&#10;</li>&#10;<li><p><code>hostname</code>: Just the lowercased hostname portion of the host.</p>&#10;<p>  Example: <code>&#39;host.com&#39;</code></p>&#10;</li>&#10;<li><p><code>port</code>: The port number portion of the host.</p>&#10;<p>  Example: <code>&#39;8080&#39;</code></p>&#10;</li>&#10;<li><p><code>pathname</code>: The path section of the URL, that comes after the host and&#10;before the query, including the initial slash if present.</p>&#10;<p>  Example: <code>&#39;/p/a/t/h&#39;</code></p>&#10;</li>&#10;<li><p><code>search</code>: The &#39;query string&#39; portion of the URL, including the leading&#10;question mark.</p>&#10;<p>  Example: <code>&#39;?query=string&#39;</code></p>&#10;</li>&#10;<li><p><code>path</code>: Concatenation of <code>pathname</code> and <code>search</code>.</p>&#10;<p>  Example: <code>&#39;/p/a/t/h?query=string&#39;</code></p>&#10;</li>&#10;<li><p><code>query</code>: Either the &#39;params&#39; portion of the query string, or a&#10;querystring-parsed object.</p>&#10;<p>  Example: <code>&#39;query=string&#39;</code> or <code>{&#39;query&#39;:&#39;string&#39;}</code></p>&#10;</li>&#10;<li><p><code>hash</code>: The &#39;fragment&#39; portion of the URL including the pound-sign.</p>&#10;<p>  Example: <code>&#39;#hash&#39;</code></p>&#10;</li>&#10;</ul>&#10;<p>The following methods are provided by the URL module:&#10;&#10;</p>&#10;</summary>
                this.parse = function(urlStr, parseQueryString, slashesDenoteHost) {
                    /// <summary><p>Take a URL string, and return an object.&#10;&#10;</p>&#10;<p>Pass <code>true</code> as the second argument to also parse&#10;the query string using the <code>querystring</code> module.&#10;Defaults to <code>false</code>.&#10;&#10;</p>&#10;<p>Pass <code>true</code> as the third argument to treat <code>//foo/bar</code> as&#10;<code>{ host: &#39;foo&#39;, pathname: &#39;/bar&#39; }</code> rather than&#10;<code>{ pathname: &#39;//foo/bar&#39; }</code>. Defaults to <code>false</code>.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="urlStr"></param>
                    /// <param name="parseQueryString"></param>
                    /// <param name="slashesDenoteHost"></param>
                    /// </signature>
                }
                this.format = function(urlObj) {
                    /// <summary><p>Take a parsed URL object, and return a formatted URL string.&#10;&#10;</p>&#10;<ul>&#10;<li><code>href</code> will be ignored.</li>&#10;<li><code>protocol</code>is treated the same with or without the trailing <code>:</code> (colon).<ul>&#10;<li>The protocols <code>http</code>, <code>https</code>, <code>ftp</code>, <code>gopher</code>, <code>file</code> will be&#10;postfixed with <code>://</code> (colon-slash-slash).</li>&#10;<li>All other protocols <code>mailto</code>, <code>xmpp</code>, <code>aim</code>, <code>sftp</code>, <code>foo</code>, etc will&#10;be postfixed with <code>:</code> (colon)</li>&#10;</ul>&#10;</li>&#10;<li><code>auth</code> will be used if present.</li>&#10;<li><code>hostname</code> will only be used if <code>host</code> is absent.</li>&#10;<li><code>port</code> will only be used if <code>host</code> is absent.</li>&#10;<li><code>host</code> will be used in place of <code>hostname</code> and <code>port</code></li>&#10;<li><code>pathname</code> is treated the same with or without the leading <code>/</code> (slash)</li>&#10;<li><code>search</code> will be used in place of <code>query</code></li>&#10;<li><code>query</code> (object; see <code>querystring</code>) will only be used if <code>search</code> is absent.</li>&#10;<li><code>search</code> is treated the same with or without the leading <code>?</code> (question mark)</li>&#10;<li><code>hash</code> is treated the same with or without the leading <code>#</code> (pound sign, anchor)</li>&#10;</ul>&#10;</summary>
                    /// <signature>
                    /// <param name="urlObj"></param>
                    /// </signature>
                }
                this.resolve = function(from, to) {
                    /// <summary><p>Take a base URL, and a href URL, and resolve them as a browser would for&#10;an anchor tag.  Examples:&#10;&#10;</p>&#10;<pre><code>url.resolve(&#39;/one/two/three&#39;, &#39;four&#39;)         // &#39;/one/two/four&#39;&#10;url.resolve(&#39;http://example.com/&#39;, &#39;/one&#39;)    // &#39;http://example.com/one&#39;&#10;url.resolve(&#39;http://example.com/one&#39;, &#39;/two&#39;) // &#39;http://example.com/two&#39;</code></pre>&#10;</summary>
                    /// <signature>
                    /// <param name="from"></param>
                    /// <param name="to"></param>
                    /// </signature>
                }
            };
            case "querystring": return new     function querystring() {
                /// <summary><p>This module provides utilities for dealing with query strings.&#10;It provides the following methods:&#10;&#10;</p>&#10;</summary>
                this.stringify = function(obj, sep, eq) {
                    /// <summary><p>Serialize an object to a query string.&#10;Optionally override the default separator (<code>&#39;&amp;&#39;</code>) and assignment (<code>&#39;=&#39;</code>)&#10;characters.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>querystring.stringify({ foo: &#39;bar&#39;, baz: [&#39;qux&#39;, &#39;quux&#39;], corge: &#39;&#39; })&#10;// returns&#10;&#39;foo=bar&amp;baz=qux&amp;baz=quux&amp;corge=&#39;&#10;&#10;querystring.stringify({foo: &#39;bar&#39;, baz: &#39;qux&#39;}, &#39;;&#39;, &#39;:&#39;)&#10;// returns&#10;&#39;foo:bar;baz:qux&#39;</code></pre>&#10;</summary>
                    /// <signature>
                    /// <param name="obj"></param>
                    /// <param name="sep"></param>
                    /// <param name="eq"></param>
                    /// </signature>
                }
                this.parse = function(str, sep, eq, options) {
                    /// <summary><p>Deserialize a query string to an object.&#10;Optionally override the default separator (<code>&#39;&amp;&#39;</code>) and assignment (<code>&#39;=&#39;</code>)&#10;characters.&#10;&#10;</p>&#10;<p>Options object may contain <code>maxKeys</code> property (equal to 1000 by default), it&#39;ll&#10;be used to limit processed keys. Set it to 0 to remove key count limitation.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>querystring.parse(&#39;foo=bar&amp;baz=qux&amp;baz=quux&amp;corge&#39;)&#10;// returns&#10;{ foo: &#39;bar&#39;, baz: [&#39;qux&#39;, &#39;quux&#39;], corge: &#39;&#39; }</code></pre>&#10;</summary>
                    /// <signature>
                    /// <param name="str"></param>
                    /// <param name="sep"></param>
                    /// <param name="eq"></param>
                    /// <param name="options"></param>
                    /// </signature>
                }
                /// <field name='escape'><p>The escape function used by <code>querystring.stringify</code>,&#10;provided so that it could be overridden if necessary.&#10;&#10;</p>&#10;</field>
                this.escape = undefined;
                /// <field name='unescape'><p>The unescape function used by <code>querystring.parse</code>,&#10;provided so that it could be overridden if necessary.&#10;&#10;</p>&#10;</field>
                this.unescape = undefined;
            };
            case "punycode": return new     function punycode() {
                /// <summary><p><a href="http://mths.be/punycode">Punycode.js</a> is bundled with Node.js v0.6.2+. Use&#10;<code>require(&#39;punycode&#39;)</code> to access it. (To use it with other Node.js versions,&#10;use npm to install the <code>punycode</code> module first.)&#10;&#10;</p>&#10;</summary>
                this.decode = function(string) {
                    /// <summary><p>Converts a Punycode string of ASCII code points to a string of Unicode code&#10;points.&#10;&#10;</p>&#10;<pre><code>// decode domain name parts&#10;punycode.decode(&#39;maana-pta&#39;); // &#39;mañana&#39;&#10;punycode.decode(&#39;--dqo34k&#39;); // &#39;☃-⌘&#39;</code></pre>&#10;</summary>
                    /// <signature>
                    /// <param name="string"></param>
                    /// </signature>
                }
                this.encode = function(string) {
                    /// <summary><p>Converts a string of Unicode code points to a Punycode string of ASCII code&#10;points.&#10;&#10;</p>&#10;<pre><code>// encode domain name parts&#10;punycode.encode(&#39;mañana&#39;); // &#39;maana-pta&#39;&#10;punycode.encode(&#39;☃-⌘&#39;); // &#39;--dqo34k&#39;</code></pre>&#10;</summary>
                    /// <signature>
                    /// <param name="string"></param>
                    /// </signature>
                }
                this.toUnicode = function(domain) {
                    /// <summary><p>Converts a Punycode string representing a domain name to Unicode. Only the&#10;Punycoded parts of the domain name will be converted, i.e. it doesn&#39;t matter if&#10;you call it on a string that has already been converted to Unicode.&#10;&#10;</p>&#10;<pre><code>// decode domain names&#10;punycode.toUnicode(&#39;xn--maana-pta.com&#39;); // &#39;mañana.com&#39;&#10;punycode.toUnicode(&#39;xn----dqo34k.com&#39;); // &#39;☃-⌘.com&#39;</code></pre>&#10;</summary>
                    /// <signature>
                    /// <param name="domain"></param>
                    /// </signature>
                }
                this.toASCII = function(domain) {
                    /// <summary><p>Converts a Unicode string representing a domain name to Punycode. Only the&#10;non-ASCII parts of the domain name will be converted, i.e. it doesn&#39;t matter if&#10;you call it with a domain that&#39;s already in ASCII.&#10;&#10;</p>&#10;<pre><code>// encode domain names&#10;punycode.toASCII(&#39;mañana.com&#39;); // &#39;xn--maana-pta.com&#39;&#10;punycode.toASCII(&#39;☃-⌘.com&#39;); // &#39;xn----dqo34k.com&#39;</code></pre>&#10;</summary>
                    /// <signature>
                    /// <param name="domain"></param>
                    /// </signature>
                }
                this.ucs2 = undefined;
                /// <field name='version'><p>A string representing the current Punycode.js version number.&#10;&#10;</p>&#10;</field>
                this.version = undefined;
            };
            case "readline": return new     function readline() {
                /// <summary><p>To use this module, do <code>require(&#39;readline&#39;)</code>. Readline allows reading of a&#10;stream (such as <code>process.stdin</code>) on a line-by-line basis.&#10;&#10;</p>&#10;<p>Note that once you&#39;ve invoked this module, your node program will not&#10;terminate until you&#39;ve closed the interface. Here&#39;s how to allow your&#10;program to gracefully exit:&#10;&#10;</p>&#10;<pre><code>var readline = require(&#39;readline&#39;);&#10;&#10;var rl = readline.createInterface({&#10;  input: process.stdin,&#10;  output: process.stdout&#10;});&#10;&#10;rl.question(&quot;What do you think of node.js? &quot;, function(answer) {&#10;  // TODO: Log the answer in a database&#10;  console.log(&quot;Thank you for your valuable feedback:&quot;, answer);&#10;&#10;  rl.close();&#10;});</code></pre>&#10;</summary>
                this.createInterface = function(options) {
                    /// <summary><p>Creates a readline <code>Interface</code> instance. Accepts an &quot;options&quot; Object that takes&#10;the following values:&#10;&#10;</p>&#10;<ul>&#10;<li><p><code>input</code> - the readable stream to listen to (Required).</p>&#10;</li>&#10;<li><p><code>output</code> - the writable stream to write readline data to (Required).</p>&#10;</li>&#10;<li><p><code>completer</code> - an optional function that is used for Tab autocompletion. See&#10;below for an example of using this.</p>&#10;</li>&#10;<li><p><code>terminal</code> - pass <code>true</code> if the <code>input</code> and <code>output</code> streams should be&#10;treated like a TTY, and have ANSI/VT100 escape codes written to it.&#10;Defaults to checking <code>isTTY</code> on the <code>output</code> stream upon instantiation.</p>&#10;</li>&#10;</ul>&#10;<p>The <code>completer</code> function is given a the current line entered by the user, and&#10;is supposed to return an Array with 2 entries:&#10;&#10;</p>&#10;<ol>&#10;<li><p>An Array with matching entries for the completion.</p>&#10;</li>&#10;<li><p>The substring that was used for the matching.</p>&#10;</li>&#10;</ol>&#10;<p>Which ends up looking something like:&#10;<code>[[substr1, substr2, ...], originalsubstring]</code>.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>function completer(line) {&#10;  var completions = &#39;.help .error .exit .quit .q&#39;.split(&#39; &#39;)&#10;  var hits = completions.filter(function(c) { return c.indexOf(line) == 0 })&#10;  // show all completions if none found&#10;  return [hits.length ? hits : completions, line]&#10;}</code></pre>&#10;<p>Also <code>completer</code> can be run in async mode if it accepts two arguments:&#10;&#10;</p>&#10;<pre><code>function completer(linePartial, callback) {&#10;  callback(null, [[&#39;123&#39;], linePartial]);&#10;}</code></pre>&#10;<p><code>createInterface</code> is commonly used with <code>process.stdin</code> and&#10;<code>process.stdout</code> in order to accept user input:&#10;&#10;</p>&#10;<pre><code>var readline = require(&#39;readline&#39;);&#10;var rl = readline.createInterface({&#10;  input: process.stdin,&#10;  output: process.stdout&#10;});</code></pre>&#10;<p>Once you have a readline instance, you most commonly listen for the&#10;<code>&quot;line&quot;</code> event.&#10;&#10;</p>&#10;<p>If <code>terminal</code> is <code>true</code> for this instance then the <code>output</code> stream will get&#10;the best compatibility if it defines an <code>output.columns</code> property, and fires&#10;a <code>&quot;resize&quot;</code> event on the <code>output</code> if/when the columns ever change&#10;(<code>process.stdout</code> does this automatically when it is a TTY).&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="options"></param>
                    /// </signature>
                    return new this.Interface();
                }
                function _Interface() {
                    this.setPrompt = function(prompt, length) {
                        /// <summary><p>Sets the prompt, for example when you run <code>node</code> on the command line, you see&#10;<code>&gt; </code>, which is node&#39;s prompt.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="prompt"></param>
                        /// <param name="length"></param>
                        /// </signature>
                    }
                    this.prompt = function(preserveCursor) {
                        /// <summary><p>Readies readline for input from the user, putting the current <code>setPrompt</code>&#10;options on a new line, giving the user a new spot to write. Set <code>preserveCursor</code>&#10;to <code>true</code> to prevent the cursor placement being reset to <code>0</code>.&#10;&#10;</p>&#10;<p>This will also resume the <code>input</code> stream used with <code>createInterface</code> if it has&#10;been paused.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="preserveCursor"></param>
                        /// </signature>
                    }
                    this.question = function(query, callback) {
                        /// <summary><p>Prepends the prompt with <code>query</code> and invokes <code>callback</code> with the user&#39;s&#10;response. Displays the query to the user, and then invokes <code>callback</code>&#10;with the user&#39;s response after it has been typed.&#10;&#10;</p>&#10;<p>This will also resume the <code>input</code> stream used with <code>createInterface</code> if&#10;it has been paused.&#10;&#10;</p>&#10;<p>Example usage:&#10;&#10;</p>&#10;<pre><code>interface.question(&#39;What is your favorite food?&#39;, function(answer) {&#10;  console.log(&#39;Oh, so your favorite food is &#39; + answer);&#10;});</code></pre>&#10;</summary>
                        /// <signature>
                        /// <param name="query"></param>
                        /// <param name="callback"></param>
                        /// </signature>
                    }
                    this.pause = function() {
                        /// <summary><p>Pauses the readline <code>input</code> stream, allowing it to be resumed later if needed.&#10;&#10;</p>&#10;</summary>
                    }
                    this.resume = function() {
                        /// <summary><p>Resumes the readline <code>input</code> stream.&#10;&#10;</p>&#10;</summary>
                    }
                    this.close = function() {
                        /// <summary><p>Closes the <code>Interface</code> instance, relinquishing control on the <code>input</code> and&#10;<code>output</code> streams. The &quot;close&quot; event will also be emitted.&#10;&#10;</p>&#10;</summary>
                    }
                    this.write = function(data, key) {
                        /// <summary><p>Writes <code>data</code> to <code>output</code> stream. <code>key</code> is an object literal to represent a key&#10;sequence; available if the terminal is a TTY.&#10;&#10;</p>&#10;<p>This will also resume the <code>input</code> stream if it has been paused.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>rl.write(&#39;Delete me!&#39;);&#10;// Simulate ctrl+u to delete the line written previously&#10;rl.write(null, {ctrl: true, name: &#39;u&#39;});</code></pre>&#10;</summary>
                        /// <signature>
                        /// <param name="data"></param>
                        /// <param name="key"></param>
                        /// </signature>
                    }
                }

                this.Interface = function() {
                    return new _Interface();
                }
            };
            case "repl": return new     function repl() {
                /// <summary><p>A Read-Eval-Print-Loop (REPL) is available both as a standalone program and&#10;easily includable in other programs. The REPL provides a way to interactively&#10;run JavaScript and see the results.  It can be used for debugging, testing, or&#10;just trying things out.&#10;&#10;</p>&#10;<p>By executing <code>node</code> without any arguments from the command-line you will be&#10;dropped into the REPL. It has simplistic emacs line-editing.&#10;&#10;</p>&#10;<pre><code>mjr:~$ node&#10;Type &#39;.help&#39; for options.&#10;&gt; a = [ 1, 2, 3];&#10;[ 1, 2, 3 ]&#10;&gt; a.forEach(function (v) {&#10;...   console.log(v);&#10;...   });&#10;1&#10;2&#10;3</code></pre>&#10;<p>For advanced line-editors, start node with the environmental variable&#10;<code>NODE_NO_READLINE=1</code>. This will start the main and debugger REPL in canonical&#10;terminal settings which will allow you to use with <code>rlwrap</code>.&#10;&#10;</p>&#10;<p>For example, you could add this to your bashrc file:&#10;&#10;</p>&#10;<pre><code>alias node=&quot;env NODE_NO_READLINE=1 rlwrap node&quot;</code></pre>&#10;</summary>
                this.start = function(options) {
                    /// <summary><p>Returns and starts a <code>REPLServer</code> instance. Accepts an &quot;options&quot; Object that&#10;takes the following values:&#10;&#10;</p>&#10;<ul>&#10;<li><p><code>prompt</code> - the prompt and <code>stream</code> for all I/O. Defaults to <code>&gt; </code>.</p>&#10;</li>&#10;<li><p><code>input</code> - the readable stream to listen to. Defaults to <code>process.stdin</code>.</p>&#10;</li>&#10;<li><p><code>output</code> - the writable stream to write readline data to. Defaults to&#10;<code>process.stdout</code>.</p>&#10;</li>&#10;<li><p><code>terminal</code> - pass <code>true</code> if the <code>stream</code> should be treated like a TTY, and&#10;have ANSI/VT100 escape codes written to it. Defaults to checking <code>isTTY</code>&#10;on the <code>output</code> stream upon instantiation.</p>&#10;</li>&#10;<li><p><code>eval</code> - function that will be used to eval each given line. Defaults to&#10;an async wrapper for <code>eval()</code>. See below for an example of a custom <code>eval</code>.</p>&#10;</li>&#10;<li><p><code>useColors</code> - a boolean which specifies whether or not the <code>writer</code> function&#10;should output colors. If a different <code>writer</code> function is set then this does&#10;nothing. Defaults to the repl&#39;s <code>terminal</code> value.</p>&#10;</li>&#10;<li><p><code>useGlobal</code> - if set to <code>true</code>, then the repl will use the <code>global</code> object,&#10;instead of running scripts in a separate context. Defaults to <code>false</code>.</p>&#10;</li>&#10;<li><p><code>ignoreUndefined</code> - if set to <code>true</code>, then the repl will not output the&#10;return value of command if it&#39;s <code>undefined</code>. Defaults to <code>false</code>.</p>&#10;</li>&#10;<li><p><code>writer</code> - the function to invoke for each command that gets evaluated which&#10;returns the formatting (including coloring) to display. Defaults to&#10;<code>util.inspect</code>.</p>&#10;</li>&#10;</ul>&#10;<p>You can use your own <code>eval</code> function if it has following signature:&#10;&#10;</p>&#10;<pre><code>function eval(cmd, context, filename, callback) {&#10;  callback(null, result);&#10;}</code></pre>&#10;<p>Multiple REPLs may be started against the same running instance of node.  Each&#10;will share the same global object but will have unique I/O.&#10;&#10;</p>&#10;<p>Here is an example that starts a REPL on stdin, a Unix socket, and a TCP socket:&#10;&#10;</p>&#10;<pre><code>var net = require(&quot;net&quot;),&#10;    repl = require(&quot;repl&quot;);&#10;&#10;connections = 0;&#10;&#10;repl.start({&#10;  prompt: &quot;node via stdin&gt; &quot;,&#10;  input: process.stdin,&#10;  output: process.stdout&#10;});&#10;&#10;net.createServer(function (socket) {&#10;  connections += 1;&#10;  repl.start({&#10;    prompt: &quot;node via Unix socket&gt; &quot;,&#10;    input: socket,&#10;    output: socket&#10;  }).on(&#39;exit&#39;, function() {&#10;    socket.end();&#10;  })&#10;}).listen(&quot;/tmp/node-repl-sock&quot;);&#10;&#10;net.createServer(function (socket) {&#10;  connections += 1;&#10;  repl.start({&#10;    prompt: &quot;node via TCP socket&gt; &quot;,&#10;    input: socket,&#10;    output: socket&#10;  }).on(&#39;exit&#39;, function() {&#10;    socket.end();&#10;  });&#10;}).listen(5001);</code></pre>&#10;<p>Running this program from the command line will start a REPL on stdin.  Other&#10;REPL clients may connect through the Unix socket or TCP socket. <code>telnet</code> is useful&#10;for connecting to TCP sockets, and <code>socat</code> can be used to connect to both Unix and&#10;TCP sockets.&#10;&#10;</p>&#10;<p>By starting a REPL from a Unix socket-based server instead of stdin, you can&#10;connect to a long-running node process without restarting it.&#10;&#10;</p>&#10;<p>For an example of running a &quot;full-featured&quot; (<code>terminal</code>) REPL over&#10;a <code>net.Server</code> and <code>net.Socket</code> instance, see: <a href="https://gist.github.com/2209310">https://gist.github.com/2209310</a>&#10;&#10;</p>&#10;<p>For an example of running a REPL instance over <code>curl(1)</code>,&#10;see: <a href="https://gist.github.com/2053342">https://gist.github.com/2053342</a>&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="options"></param>
                    /// </signature>
                }
            };
            case "vm": return new     function vm() {
                /// <summary><p>You can access this module with:&#10;&#10;</p>&#10;<pre><code>var vm = require(&#39;vm&#39;);</code></pre>&#10;<p>JavaScript code can be compiled and run immediately or compiled, saved, and run later.&#10;&#10;</p>&#10;</summary>
                this.runInThisContext = function(code, filename) {
                    /// <summary><p><code>vm.runInThisContext()</code> compiles <code>code</code>, runs it and returns the result. Running&#10;code does not have access to local scope. <code>filename</code> is optional, it&#39;s used only&#10;in stack traces.&#10;&#10;</p>&#10;<p>Example of using <code>vm.runInThisContext</code> and <code>eval</code> to run the same code:&#10;&#10;</p>&#10;<pre><code>var localVar = 123,&#10;    usingscript, evaled,&#10;    vm = require(&#39;vm&#39;);&#10;&#10;usingscript = vm.runInThisContext(&#39;localVar = 1;&#39;,&#10;  &#39;myfile.vm&#39;);&#10;console.log(&#39;localVar: &#39; + localVar + &#39;, usingscript: &#39; +&#10;  usingscript);&#10;evaled = eval(&#39;localVar = 1;&#39;);&#10;console.log(&#39;localVar: &#39; + localVar + &#39;, evaled: &#39; +&#10;  evaled);&#10;&#10;// localVar: 123, usingscript: 1&#10;// localVar: 1, evaled: 1</code></pre>&#10;<p><code>vm.runInThisContext</code> does not have access to the local scope, so <code>localVar</code> is unchanged.&#10;<code>eval</code> does have access to the local scope, so <code>localVar</code> is changed.&#10;&#10;</p>&#10;<p>In case of syntax error in <code>code</code>, <code>vm.runInThisContext</code> emits the syntax error to stderr&#10;and throws an exception.&#10;&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="code"></param>
                    /// <param name="filename"></param>
                    /// </signature>
                }
                this.runInNewContext = function(code, sandbox, filename) {
                    /// <summary><p><code>vm.runInNewContext</code> compiles <code>code</code>, then runs it in <code>sandbox</code> and returns the&#10;result. Running code does not have access to local scope. The object <code>sandbox</code>&#10;will be used as the global object for <code>code</code>.&#10;<code>sandbox</code> and <code>filename</code> are optional, <code>filename</code> is only used in stack traces.&#10;&#10;</p>&#10;<p>Example: compile and execute code that increments a global variable and sets a new one.&#10;These globals are contained in the sandbox.&#10;&#10;</p>&#10;<pre><code>var util = require(&#39;util&#39;),&#10;    vm = require(&#39;vm&#39;),&#10;    sandbox = {&#10;      animal: &#39;cat&#39;,&#10;      count: 2&#10;    };&#10;&#10;vm.runInNewContext(&#39;count += 1; name = &quot;kitty&quot;&#39;, sandbox, &#39;myfile.vm&#39;);&#10;console.log(util.inspect(sandbox));&#10;&#10;// { animal: &#39;cat&#39;, count: 3, name: &#39;kitty&#39; }</code></pre>&#10;<p>Note that running untrusted code is a tricky business requiring great care.  To prevent accidental&#10;global variable leakage, <code>vm.runInNewContext</code> is quite useful, but safely running untrusted code&#10;requires a separate process.&#10;&#10;</p>&#10;<p>In case of syntax error in <code>code</code>, <code>vm.runInNewContext</code> emits the syntax error to stderr&#10;and throws an exception.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="code"></param>
                    /// <param name="sandbox"></param>
                    /// <param name="filename"></param>
                    /// </signature>
                }
                this.runInContext = function(code, context, filename) {
                    /// <summary><p><code>vm.runInContext</code> compiles <code>code</code>, then runs it in <code>context</code> and returns the&#10;result. A (V8) context comprises a global object, together with a set of&#10;built-in objects and functions. Running code does not have access to local scope&#10;and the global object held within <code>context</code> will be used as the global object&#10;for <code>code</code>.&#10;<code>filename</code> is optional, it&#39;s used only in stack traces.&#10;&#10;</p>&#10;<p>Example: compile and execute code in a existing context.&#10;&#10;</p>&#10;<pre><code>var util = require(&#39;util&#39;),&#10;    vm = require(&#39;vm&#39;),&#10;    initSandbox = {&#10;      animal: &#39;cat&#39;,&#10;      count: 2&#10;    },&#10;    context = vm.createContext(initSandbox);&#10;&#10;vm.runInContext(&#39;count += 1; name = &quot;CATT&quot;&#39;, context, &#39;myfile.vm&#39;);&#10;console.log(util.inspect(context));&#10;&#10;// { animal: &#39;cat&#39;, count: 3, name: &#39;CATT&#39; }</code></pre>&#10;<p>Note that <code>createContext</code> will perform a shallow clone of the supplied sandbox object in order to&#10;initialize the global object of the freshly constructed context.&#10;&#10;</p>&#10;<p>Note that running untrusted code is a tricky business requiring great care.  To prevent accidental&#10;global variable leakage, <code>vm.runInContext</code> is quite useful, but safely running untrusted code&#10;requires a separate process.&#10;&#10;</p>&#10;<p>In case of syntax error in <code>code</code>, <code>vm.runInContext</code> emits the syntax error to stderr&#10;and throws an exception.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="code"></param>
                    /// <param name="context"></param>
                    /// <param name="filename"></param>
                    /// </signature>
                }
                this.createContext = function(initSandbox) {
                    /// <summary><p><code>vm.createContext</code> creates a new context which is suitable for use as the 2nd argument of a subsequent&#10;call to <code>vm.runInContext</code>. A (V8) context comprises a global object together with a set of&#10;build-in objects and functions. The optional argument <code>initSandbox</code> will be shallow-copied&#10;to seed the initial contents of the global object used by the context.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="initSandbox"></param>
                    /// </signature>
                    return new this.Context();
                }
                this.createScript = function(code, filename) {
                    /// <summary><p><code>createScript</code> compiles <code>code</code> but does not run it. Instead, it returns a&#10;<code>vm.Script</code> object representing this compiled code. This script can be run&#10;later many times using methods below. The returned script is not bound to any&#10;global object. It is bound before each run, just for that run. <code>filename</code> is&#10;optional, it&#39;s only used in stack traces.&#10;&#10;</p>&#10;<p>In case of syntax error in <code>code</code>, <code>createScript</code> prints the syntax error to stderr&#10;and throws an exception.&#10;&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="code"></param>
                    /// <param name="filename"></param>
                    /// </signature>
                    return new this.Script();
                }
                function _Script() {
                    this.runInThisContext = function() {
                        /// <summary><p>Similar to <code>vm.runInThisContext</code> but a method of a precompiled <code>Script</code> object.&#10;<code>script.runInThisContext</code> runs the code of <code>script</code> and returns the result.&#10;Running code does not have access to local scope, but does have access to the <code>global</code> object&#10;(v8: in actual context).&#10;&#10;</p>&#10;<p>Example of using <code>script.runInThisContext</code> to compile code once and run it multiple times:&#10;&#10;</p>&#10;<pre><code>var vm = require(&#39;vm&#39;);&#10;&#10;globalVar = 0;&#10;&#10;var script = vm.createScript(&#39;globalVar += 1&#39;, &#39;myfile.vm&#39;);&#10;&#10;for (var i = 0; i &lt; 1000 ; i += 1) {&#10;  script.runInThisContext();&#10;}&#10;&#10;console.log(globalVar);&#10;&#10;// 1000</code></pre>&#10;</summary>
                    }
                    this.runInNewContext = function(sandbox) {
                        /// <summary><p>Similar to <code>vm.runInNewContext</code> a method of a precompiled <code>Script</code> object.&#10;<code>script.runInNewContext</code> runs the code of <code>script</code> with <code>sandbox</code> as the global object and returns the result.&#10;Running code does not have access to local scope. <code>sandbox</code> is optional.&#10;&#10;</p>&#10;<p>Example: compile code that increments a global variable and sets one, then execute this code multiple times.&#10;These globals are contained in the sandbox.&#10;&#10;</p>&#10;<pre><code>var util = require(&#39;util&#39;),&#10;    vm = require(&#39;vm&#39;),&#10;    sandbox = {&#10;      animal: &#39;cat&#39;,&#10;      count: 2&#10;    };&#10;&#10;var script = vm.createScript(&#39;count += 1; name = &quot;kitty&quot;&#39;, &#39;myfile.vm&#39;);&#10;&#10;for (var i = 0; i &lt; 10 ; i += 1) {&#10;  script.runInNewContext(sandbox);&#10;}&#10;&#10;console.log(util.inspect(sandbox));&#10;&#10;// { animal: &#39;cat&#39;, count: 12, name: &#39;kitty&#39; }</code></pre>&#10;<p>Note that running untrusted code is a tricky business requiring great care.  To prevent accidental&#10;global variable leakage, <code>script.runInNewContext</code> is quite useful, but safely running untrusted code&#10;requires a separate process.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="sandbox"></param>
                        /// </signature>
                    }
                }

                this.Script = function() {
                    return new _Script();
                }
            };
            case "child_process": return new     function child_process() {
                /// <summary><p>Node provides a tri-directional <code>popen(3)</code> facility through the&#10;<code>child_process</code> module.&#10;&#10;</p>&#10;<p>It is possible to stream data through a child&#39;s <code>stdin</code>, <code>stdout</code>, and&#10;<code>stderr</code> in a fully non-blocking way.  (Note that some programs use&#10;line-buffered I/O internally.  That doesn&#39;t affect node.js but it means&#10;data you send to the child process is not immediately consumed.)&#10;&#10;</p>&#10;<p>To create a child process use <code>require(&#39;child_process&#39;).spawn()</code> or&#10;<code>require(&#39;child_process&#39;).fork()</code>.  The semantics of each are slightly&#10;different, and explained below.&#10;&#10;</p>&#10;</summary>
                this.spawn = function(command, args, options) {
                    /// <summary><p>Launches a new process with the given <code>command</code>, with  command line arguments in <code>args</code>.&#10;If omitted, <code>args</code> defaults to an empty Array.&#10;&#10;</p>&#10;<p>The third argument is used to specify additional options, which defaults to:&#10;&#10;</p>&#10;<pre><code>{ cwd: undefined,&#10;  env: process.env&#10;}</code></pre>&#10;<p><code>cwd</code> allows you to specify the working directory from which the process is spawned.&#10;Use <code>env</code> to specify environment variables that will be visible to the new process.&#10;&#10;</p>&#10;<p>Example of running <code>ls -lh /usr</code>, capturing <code>stdout</code>, <code>stderr</code>, and the exit code:&#10;&#10;</p>&#10;<pre><code>var spawn = require(&#39;child_process&#39;).spawn,&#10;    ls    = spawn(&#39;ls&#39;, [&#39;-lh&#39;, &#39;/usr&#39;]);&#10;&#10;ls.stdout.on(&#39;data&#39;, function (data) {&#10;  console.log(&#39;stdout: &#39; + data);&#10;});&#10;&#10;ls.stderr.on(&#39;data&#39;, function (data) {&#10;  console.log(&#39;stderr: &#39; + data);&#10;});&#10;&#10;ls.on(&#39;close&#39;, function (code) {&#10;  console.log(&#39;child process exited with code &#39; + code);&#10;});</code></pre>&#10;<p>Example: A very elaborate way to run &#39;ps ax | grep ssh&#39;&#10;&#10;</p>&#10;<pre><code>var spawn = require(&#39;child_process&#39;).spawn,&#10;    ps    = spawn(&#39;ps&#39;, [&#39;ax&#39;]),&#10;    grep  = spawn(&#39;grep&#39;, [&#39;ssh&#39;]);&#10;&#10;ps.stdout.on(&#39;data&#39;, function (data) {&#10;  grep.stdin.write(data);&#10;});&#10;&#10;ps.stderr.on(&#39;data&#39;, function (data) {&#10;  console.log(&#39;ps stderr: &#39; + data);&#10;});&#10;&#10;ps.on(&#39;close&#39;, function (code) {&#10;  if (code !== 0) {&#10;    console.log(&#39;ps process exited with code &#39; + code);&#10;  }&#10;  grep.stdin.end();&#10;});&#10;&#10;grep.stdout.on(&#39;data&#39;, function (data) {&#10;  console.log(&#39;&#39; + data);&#10;});&#10;&#10;grep.stderr.on(&#39;data&#39;, function (data) {&#10;  console.log(&#39;grep stderr: &#39; + data);&#10;});&#10;&#10;grep.on(&#39;close&#39;, function (code) {&#10;  if (code !== 0) {&#10;    console.log(&#39;grep process exited with code &#39; + code);&#10;  }&#10;});</code></pre>&#10;<p>Example of checking for failed exec:&#10;&#10;</p>&#10;<pre><code>var spawn = require(&#39;child_process&#39;).spawn,&#10;    child = spawn(&#39;bad_command&#39;);&#10;&#10;child.stderr.setEncoding(&#39;utf8&#39;);&#10;child.stderr.on(&#39;data&#39;, function (data) {&#10;  if (/^execvp\(\)/.test(data)) {&#10;    console.log(&#39;Failed to start child process.&#39;);&#10;  }&#10;});</code></pre>&#10;<p>Note that if spawn receives an empty options object, it will result in&#10;spawning the process with an empty environment rather than using&#10;<code>process.env</code>. This due to backwards compatibility issues with a deprecated&#10;API.&#10;&#10;</p>&#10;<p>The &#39;stdio&#39; option to <code>child_process.spawn()</code> is an array where each&#10;index corresponds to a fd in the child.  The value is one of the following:&#10;&#10;</p>&#10;<ol>&#10;<li><code>&#39;pipe&#39;</code> - Create a pipe between the child process and the parent process.&#10;The parent end of the pipe is exposed to the parent as a property on the&#10;<code>child_process</code> object as <code>ChildProcess.stdio[fd]</code>. Pipes created for&#10;fds 0 - 2 are also available as ChildProcess.stdin, ChildProcess.stdout&#10;and ChildProcess.stderr, respectively.</li>&#10;<li><code>&#39;ipc&#39;</code> - Create an IPC channel for passing messages/file descriptors&#10;between parent and child. A ChildProcess may have at most <em>one</em> IPC stdio&#10;file descriptor. Setting this option enables the ChildProcess.send() method.&#10;If the child writes JSON messages to this file descriptor, then this will&#10;trigger ChildProcess.on(&#39;message&#39;).  If the child is a Node.js program, then&#10;the presence of an IPC channel will enable process.send() and&#10;process.on(&#39;message&#39;).</li>&#10;<li><code>&#39;ignore&#39;</code> - Do not set this file descriptor in the child. Note that Node&#10;will always open fd 0 - 2 for the processes it spawns. When any of these is&#10;ignored node will open <code>/dev/null</code> and attach it to the child&#39;s fd.</li>&#10;<li><code>Stream</code> object - Share a readable or writable stream that refers to a tty,&#10;file, socket, or a pipe with the child process. The stream&#39;s underlying&#10;file descriptor is duplicated in the child process to the fd that &#10;corresponds to the index in the <code>stdio</code> array.</li>&#10;<li>Positive integer - The integer value is interpreted as a file descriptor &#10;that is is currently open in the parent process. It is shared with the child&#10;process, similar to how <code>Stream</code> objects can be shared.</li>&#10;<li><code>null</code>, <code>undefined</code> - Use default value. For stdio fds 0, 1 and 2 (in other&#10;words, stdin, stdout, and stderr) a pipe is created. For fd 3 and up, the&#10;default is <code>&#39;ignore&#39;</code>.</li>&#10;</ol>&#10;<p>As a shorthand, the <code>stdio</code> argument may also be one of the following&#10;strings, rather than an array:&#10;&#10;</p>&#10;<ul>&#10;<li><code>ignore</code> - <code>[&#39;ignore&#39;, &#39;ignore&#39;, &#39;ignore&#39;]</code></li>&#10;<li><code>pipe</code> - <code>[&#39;pipe&#39;, &#39;pipe&#39;, &#39;pipe&#39;]</code></li>&#10;<li><code>inherit</code> - <code>[process.stdin, process.stdout, process.stderr]</code> or <code>[0,1,2]</code></li>&#10;</ul>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>var spawn = require(&#39;child_process&#39;).spawn;&#10;&#10;// Child will use parent&#39;s stdios&#10;spawn(&#39;prg&#39;, [], { stdio: &#39;inherit&#39; });&#10;&#10;// Spawn child sharing only stderr&#10;spawn(&#39;prg&#39;, [], { stdio: [&#39;pipe&#39;, &#39;pipe&#39;, process.stderr] });&#10;&#10;// Open an extra fd=4, to interact with programs present a&#10;// startd-style interface.&#10;spawn(&#39;prg&#39;, [], { stdio: [&#39;pipe&#39;, null, null, null, &#39;pipe&#39;] });</code></pre>&#10;<p>If the <code>detached</code> option is set, the child process will be made the leader of a&#10;new process group.  This makes it possible for the child to continue running &#10;after the parent exits.&#10;&#10;</p>&#10;<p>By default, the parent will wait for the detached child to exit.  To prevent&#10;the parent from waiting for a given <code>child</code>, use the <code>child.unref()</code> method,&#10;and the parent&#39;s event loop will not include the child in its reference count.&#10;&#10;</p>&#10;<p>Example of detaching a long-running process and redirecting its output to a&#10;file:&#10;&#10;</p>&#10;<pre><code> var fs = require(&#39;fs&#39;),&#10;     spawn = require(&#39;child_process&#39;).spawn,&#10;     out = fs.openSync(&#39;./out.log&#39;, &#39;a&#39;),&#10;     err = fs.openSync(&#39;./out.log&#39;, &#39;a&#39;);&#10;&#10; var child = spawn(&#39;prg&#39;, [], {&#10;   detached: true,&#10;   stdio: [ &#39;ignore&#39;, out, err ]&#10; });&#10;&#10; child.unref();</code></pre>&#10;<p>When using the <code>detached</code> option to start a long-running process, the process&#10;will not stay running in the background unless it is provided with a <code>stdio</code>&#10;configuration that is not connected to the parent.  If the parent&#39;s <code>stdio</code> is&#10;inherited, the child will remain attached to the controlling terminal.&#10;&#10;</p>&#10;<p>There is a deprecated option called <code>customFds</code> which allows one to specify&#10;specific file descriptors for the stdio of the child process. This API was&#10;not portable to all platforms and therefore removed.&#10;With <code>customFds</code> it was possible to hook up the new process&#39; <code>[stdin, stdout,&#10;stderr]</code> to existing streams; <code>-1</code> meant that a new stream should be created.&#10;Use at your own risk.&#10;&#10;</p>&#10;<p>See also: <code>child_process.exec()</code> and <code>child_process.fork()</code>&#10;&#10;</p>&#10;</summary>
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
                    /// <summary><p>Runs a command in a shell and buffers the output.&#10;&#10;</p>&#10;<pre><code>var exec = require(&#39;child_process&#39;).exec,&#10;    child;&#10;&#10;child = exec(&#39;cat *.js bad_file | wc -l&#39;,&#10;  function (error, stdout, stderr) {&#10;    console.log(&#39;stdout: &#39; + stdout);&#10;    console.log(&#39;stderr: &#39; + stderr);&#10;    if (error !== null) {&#10;      console.log(&#39;exec error: &#39; + error);&#10;    }&#10;});</code></pre>&#10;<p>The callback gets the arguments <code>(error, stdout, stderr)</code>. On success, <code>error</code>&#10;will be <code>null</code>.  On error, <code>error</code> will be an instance of <code>Error</code> and <code>err.code</code>&#10;will be the exit code of the child process, and <code>err.signal</code> will be set to the&#10;signal that terminated the process.&#10;&#10;</p>&#10;<p>There is a second optional argument to specify several options. The&#10;default options are&#10;&#10;</p>&#10;<pre><code>{ encoding: &#39;utf8&#39;,&#10;  timeout: 0,&#10;  maxBuffer: 200*1024,&#10;  killSignal: &#39;SIGTERM&#39;,&#10;  cwd: null,&#10;  env: null }</code></pre>&#10;<p>If <code>timeout</code> is greater than 0, then it will kill the child process&#10;if it runs longer than <code>timeout</code> milliseconds. The child process is killed with&#10;<code>killSignal</code> (default: <code>&#39;SIGTERM&#39;</code>). <code>maxBuffer</code> specifies the largest&#10;amount of data allowed on stdout or stderr - if this value is exceeded then&#10;the child process is killed.&#10;&#10;&#10;</p>&#10;</summary>
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
                    /// <summary><p>This is similar to <code>child_process.exec()</code> except it does not execute a&#10;subshell but rather the specified file directly. This makes it slightly&#10;leaner than <code>child_process.exec</code>. It has the same options.&#10;&#10;&#10;</p>&#10;</summary>
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
                    /// <summary><p>This is a special case of the <code>spawn()</code> functionality for spawning Node&#10;processes. In addition to having all the methods in a normal ChildProcess&#10;instance, the returned object has a communication channel built-in. See&#10;<code>child.send(message, [sendHandle])</code> for details.&#10;&#10;</p>&#10;<p>By default the spawned Node process will have the stdout, stderr associated&#10;with the parent&#39;s. To change this behavior set the <code>silent</code> property in the&#10;<code>options</code> object to <code>true</code>.&#10;&#10;</p>&#10;<p>The child process does not automatically exit once it&#39;s done, you need to call&#10;<code>process.exit()</code> explicitly. This limitation may be lifted in the future.&#10;&#10;</p>&#10;<p>These child Nodes are still whole new instances of V8. Assume at least 30ms&#10;startup and 10mb memory for each new Node. That is, you cannot create many&#10;thousands of them.&#10;&#10;</p>&#10;<p>The <code>execPath</code> property in the <code>options</code> object allows for a process to be&#10;created for the child rather than the current <code>node</code> executable. This should be&#10;done with care and by default will talk over the fd represented an&#10;environmental variable <code>NODE_CHANNEL_FD</code> on the child process. The input and&#10;output on this fd is expected to be line delimited JSON objects.&#10;&#10;</p>&#10;</summary>
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
                function _ChildProcess() {
                    this.kill = function(signal) {
                        /// <summary><p>Send a signal to the child process. If no argument is given, the process will&#10;be sent <code>&#39;SIGTERM&#39;</code>. See <code>signal(7)</code> for a list of available signals.&#10;&#10;</p>&#10;<pre><code>var spawn = require(&#39;child_process&#39;).spawn,&#10;    grep  = spawn(&#39;grep&#39;, [&#39;ssh&#39;]);&#10;&#10;grep.on(&#39;close&#39;, function (code, signal) {&#10;  console.log(&#39;child process terminated due to receipt of signal &#39;+signal);&#10;});&#10;&#10;// send SIGHUP to process&#10;grep.kill(&#39;SIGHUP&#39;);</code></pre>&#10;<p>May emit an <code>&#39;error&#39;</code> event when the signal cannot be delivered. Sending a&#10;signal to a child process that has already exited is not an error but may&#10;have unforeseen consequences: if the PID (the process ID) has been reassigned&#10;to another process, the signal will be delivered to that process instead.&#10;What happens next is anyone&#39;s guess.&#10;&#10;</p>&#10;<p>Note that while the function is called <code>kill</code>, the signal delivered to the&#10;child process may not actually kill it.  <code>kill</code> really just sends a signal&#10;to a process.&#10;&#10;</p>&#10;<p>See <code>kill(2)</code>&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="signal" type="String"></param>
                        /// </signature>
                        /// <signature>
                        /// <param name="signal"></param>
                        /// </signature>
                    }
                    this.send = function(message, sendHandle) {
                        /// <summary><p>When using <code>child_process.fork()</code> you can write to the child using&#10;<code>child.send(message, [sendHandle])</code> and messages are received by&#10;a <code>&#39;message&#39;</code> event on the child.&#10;&#10;</p>&#10;<p>For example:&#10;&#10;</p>&#10;<pre><code>var cp = require(&#39;child_process&#39;);&#10;&#10;var n = cp.fork(__dirname + &#39;/sub.js&#39;);&#10;&#10;n.on(&#39;message&#39;, function(m) {&#10;  console.log(&#39;PARENT got message:&#39;, m);&#10;});&#10;&#10;n.send({ hello: &#39;world&#39; });</code></pre>&#10;<p>And then the child script, <code>&#39;sub.js&#39;</code> might look like this:&#10;&#10;</p>&#10;<pre><code>process.on(&#39;message&#39;, function(m) {&#10;  console.log(&#39;CHILD got message:&#39;, m);&#10;});&#10;&#10;process.send({ foo: &#39;bar&#39; });</code></pre>&#10;<p>In the child the <code>process</code> object will have a <code>send()</code> method, and <code>process</code>&#10;will emit objects each time it receives a message on its channel.&#10;&#10;</p>&#10;<p>There is a special case when sending a <code>{cmd: &#39;NODE_foo&#39;}</code> message. All messages&#10;containing a <code>NODE_</code> prefix in its <code>cmd</code> property will not be emitted in&#10;the <code>message</code> event, since they are internal messages used by node core.&#10;Messages containing the prefix are emitted in the <code>internalMessage</code> event, you&#10;should by all means avoid using this feature, it is subject to change without notice.&#10;&#10;</p>&#10;<p>The <code>sendHandle</code> option to <code>child.send()</code> is for sending a TCP server or&#10;socket object to another process. The child will receive the object as its&#10;second argument to the <code>message</code> event.&#10;&#10;</p>&#10;<p>Emits an <code>&#39;error&#39;</code> event if the message cannot be sent, for example because&#10;the child process has already exited.&#10;&#10;</p>&#10;<h4>Example: sending server object</h4>&#10;<p>Here is an example of sending a server:&#10;&#10;</p>&#10;<pre><code>var child = require(&#39;child_process&#39;).fork(&#39;child.js&#39;);&#10;&#10;// Open up the server object and send the handle.&#10;var server = require(&#39;net&#39;).createServer();&#10;server.on(&#39;connection&#39;, function (socket) {&#10;  socket.end(&#39;handled by parent&#39;);&#10;});&#10;server.listen(1337, function() {&#10;  child.send(&#39;server&#39;, server);&#10;});</code></pre>&#10;<p>And the child would the receive the server object as:&#10;&#10;</p>&#10;<pre><code>process.on(&#39;message&#39;, function(m, server) {&#10;  if (m === &#39;server&#39;) {&#10;    server.on(&#39;connection&#39;, function (socket) {&#10;      socket.end(&#39;handled by child&#39;);&#10;    });&#10;  }&#10;});</code></pre>&#10;<p>Note that the server is now shared between the parent and child, this means&#10;that some connections will be handled by the parent and some by the child.&#10;&#10;</p>&#10;<p>For <code>dgram</code> servers the workflow is exactly the same.  Here you listen on&#10;a <code>message</code> event instead of <code>connection</code> and use <code>server.bind</code> instead of&#10;<code>server.listen</code>.  (Currently only supported on UNIX platforms.)&#10;&#10;</p>&#10;<h4>Example: sending socket object</h4>&#10;<p>Here is an example of sending a socket. It will spawn two children and handle&#10;connections with the remote address <code>74.125.127.100</code> as VIP by sending the&#10;socket to a &quot;special&quot; child process. Other sockets will go to a &quot;normal&quot; process.&#10;&#10;</p>&#10;<pre><code>var normal = require(&#39;child_process&#39;).fork(&#39;child.js&#39;, [&#39;normal&#39;]);&#10;var special = require(&#39;child_process&#39;).fork(&#39;child.js&#39;, [&#39;special&#39;]);&#10;&#10;// Open up the server and send sockets to child&#10;var server = require(&#39;net&#39;).createServer();&#10;server.on(&#39;connection&#39;, function (socket) {&#10;&#10;  // if this is a VIP&#10;  if (socket.remoteAddress === &#39;74.125.127.100&#39;) {&#10;    special.send(&#39;socket&#39;, socket);&#10;    return;&#10;  }&#10;  // just the usual dudes&#10;  normal.send(&#39;socket&#39;, socket);&#10;});&#10;server.listen(1337);</code></pre>&#10;<p>The <code>child.js</code> could look like this:&#10;&#10;</p>&#10;<pre><code>process.on(&#39;message&#39;, function(m, socket) {&#10;  if (m === &#39;socket&#39;) {&#10;    socket.end(&#39;You were handled as a &#39; + process.argv[2] + &#39; person&#39;);&#10;  }&#10;});</code></pre>&#10;<p>Note that once a single socket has been sent to a child the parent can no&#10;longer keep track of when the socket is destroyed. To indicate this condition&#10;the <code>.connections</code> property becomes <code>null</code>.&#10;It is also recommended not to use <code>.maxConnections</code> in this condition.&#10;&#10;</p>&#10;</summary>
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
                        /// <summary><p>To close the IPC connection between parent and child use the&#10;<code>child.disconnect()</code> method. This allows the child to exit gracefully since&#10;there is no IPC channel keeping it alive. When calling this method the&#10;<code>disconnect</code> event will be emitted in both parent and child, and the&#10;<code>connected</code> flag will be set to <code>false</code>. Please note that you can also call&#10;<code>process.disconnect()</code> in the child process.&#10;&#10;</p>&#10;</summary>
                    }
                    this.addListener = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// error: Emitted when: ...&#10;
                        /// exit: This event is emitted after the child process ends. If the process terminated ...&#10;
                        /// close: This event is emitted when the stdio streams of a child process have all ...&#10;
                        /// disconnect: This event is emitted after using the <code>.disconnect()</code> method in the parent or ...&#10;
                        /// message: Messages send by <code>.send(message, [sendHandle])</code> are obtained using the ...&#10;
                        /// </summary>

                    }
                    this.once = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// error: Emitted when: ...&#10;
                        /// exit: This event is emitted after the child process ends. If the process terminated ...&#10;
                        /// close: This event is emitted when the stdio streams of a child process have all ...&#10;
                        /// disconnect: This event is emitted after using the <code>.disconnect()</code> method in the parent or ...&#10;
                        /// message: Messages send by <code>.send(message, [sendHandle])</code> are obtained using the ...&#10;
                        /// </summary>

                    }
                    this.removeListener = function(event, listener) {
                        /// <summary>Supported Events: error, exit, close, disconnect, message</summary>
                    }
                    this.removeAllListeners = function(event) {
                        /// <summary>Supported Events: error, exit, close, disconnect, message</summary>
                    }
                    this.setMaxListeners = function(n) { }
                    this.listeners = function(event) {
                        /// <summary>Supported Events: error, exit, close, disconnect, message</summary>
                    }
                    this.emit = function(event, arguments) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// error: Emitted when: ...&#10;
                        /// exit: This event is emitted after the child process ends. If the process terminated ...&#10;
                        /// close: This event is emitted when the stdio streams of a child process have all ...&#10;
                        /// disconnect: This event is emitted after using the <code>.disconnect()</code> method in the parent or ...&#10;
                        /// message: Messages send by <code>.send(message, [sendHandle])</code> are obtained using the ...&#10;
                        /// </summary>

                    }
                    this.on = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// error: Emitted when: ...&#10;
                        /// exit: This event is emitted after the child process ends. If the process terminated ...&#10;
                        /// close: This event is emitted when the stdio streams of a child process have all ...&#10;
                        /// disconnect: This event is emitted after using the <code>.disconnect()</code> method in the parent or ...&#10;
                        /// message: Messages send by <code>.send(message, [sendHandle])</code> are obtained using the ...&#10;
                        /// </summary>

                    }
                    /// <field name='stdin'><p>A <code>Writable Stream</code> that represents the child process&#39;s <code>stdin</code>.&#10;Closing this stream via <code>end()</code> often causes the child process to terminate.&#10;&#10;</p>&#10;<p>If the child stdio streams are shared with the parent, then this will&#10;not be set.&#10;&#10;</p>&#10;</field>
                    this.stdin = require('stream').Writable();
                    /// <field name='stdout'><p>A <code>Readable Stream</code> that represents the child process&#39;s <code>stdout</code>.&#10;&#10;</p>&#10;<p>If the child stdio streams are shared with the parent, then this will&#10;not be set.&#10;&#10;</p>&#10;</field>
                    this.stdout = require('stream').Readable();
                    /// <field name='stderr'><p>A <code>Readable Stream</code> that represents the child process&#39;s <code>stderr</code>.&#10;&#10;</p>&#10;<p>If the child stdio streams are shared with the parent, then this will&#10;not be set.&#10;&#10;</p>&#10;</field>
                    this.stderr = require('stream').Readable();
                    /// <field name='pid'><p>The PID of the child process.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>var spawn = require(&#39;child_process&#39;).spawn,&#10;    grep  = spawn(&#39;grep&#39;, [&#39;ssh&#39;]);&#10;&#10;console.log(&#39;Spawned child pid: &#39; + grep.pid);&#10;grep.stdin.end();</code></pre>&#10;</field>
                    this.pid = undefined;
                }

                this.ChildProcess = function() {
                    return new _ChildProcess();
                }
            };
            case "assert": return new     function assert() {
                /// <summary><p>This module is used for writing unit tests for your applications, you can&#10;access it with <code>require(&#39;assert&#39;)</code>.&#10;&#10;</p>&#10;</summary>
                this.fail = function(actual, expected, message, operator) {
                    /// <summary><p>Throws an exception that displays the values for <code>actual</code> and <code>expected</code> separated by the provided operator.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="actual"></param>
                    /// <param name="expected"></param>
                    /// <param name="message"></param>
                    /// <param name="operator"></param>
                    /// </signature>
                }
                this.ok = function(value, message, assert, message) {
                    /// <summary><p>Tests if value is truthy, it is equivalent to <code>assert.equal(true, !!value, message);</code>&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="value"></param>
                    /// <param name="message)"></param>
                    /// <param name="assert.ok(value"></param>
                    /// <param name="message"></param>
                    /// </signature>
                }
                this.equal = function(actual, expected, message) {
                    /// <summary><p>Tests shallow, coercive equality with the equal comparison operator ( <code>==</code> ).&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="actual"></param>
                    /// <param name="expected"></param>
                    /// <param name="message"></param>
                    /// </signature>
                }
                this.notEqual = function(actual, expected, message) {
                    /// <summary><p>Tests shallow, coercive non-equality with the not equal comparison operator ( <code>!=</code> ).&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="actual"></param>
                    /// <param name="expected"></param>
                    /// <param name="message"></param>
                    /// </signature>
                }
                this.deepEqual = function(actual, expected, message) {
                    /// <summary><p>Tests for deep equality.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="actual"></param>
                    /// <param name="expected"></param>
                    /// <param name="message"></param>
                    /// </signature>
                }
                this.notDeepEqual = function(actual, expected, message) {
                    /// <summary><p>Tests for any deep inequality.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="actual"></param>
                    /// <param name="expected"></param>
                    /// <param name="message"></param>
                    /// </signature>
                }
                this.strictEqual = function(actual, expected, message) {
                    /// <summary><p>Tests strict equality, as determined by the strict equality operator ( <code>===</code> )&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="actual"></param>
                    /// <param name="expected"></param>
                    /// <param name="message"></param>
                    /// </signature>
                }
                this.notStrictEqual = function(actual, expected, message) {
                    /// <summary><p>Tests strict non-equality, as determined by the strict not equal operator ( <code>!==</code> )&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="actual"></param>
                    /// <param name="expected"></param>
                    /// <param name="message"></param>
                    /// </signature>
                }
                this.throws = function(block, error, message) {
                    /// <summary><p>Expects <code>block</code> to throw an error. <code>error</code> can be constructor, regexp or &#10;validation function.&#10;&#10;</p>&#10;<p>Validate instanceof using constructor:&#10;&#10;</p>&#10;<pre><code>assert.throws(&#10;  function() {&#10;    throw new Error(&quot;Wrong value&quot;);&#10;  },&#10;  Error&#10;);</code></pre>&#10;<p>Validate error message using RegExp:&#10;&#10;</p>&#10;<pre><code>assert.throws(&#10;  function() {&#10;    throw new Error(&quot;Wrong value&quot;);&#10;  },&#10;  /value/&#10;);</code></pre>&#10;<p>Custom error validation:&#10;&#10;</p>&#10;<pre><code>assert.throws(&#10;  function() {&#10;    throw new Error(&quot;Wrong value&quot;);&#10;  },&#10;  function(err) {&#10;    if ( (err instanceof Error) &amp;&amp; /value/.test(err) ) {&#10;      return true;&#10;    }&#10;  },&#10;  &quot;unexpected error&quot;&#10;);</code></pre>&#10;</summary>
                    /// <signature>
                    /// <param name="block"></param>
                    /// <param name="error"></param>
                    /// <param name="message"></param>
                    /// </signature>
                }
                this.doesNotThrow = function(block, message) {
                    /// <summary><p>Expects <code>block</code> not to throw an error, see assert.throws for details.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="block"></param>
                    /// <param name="message"></param>
                    /// </signature>
                }
                this.ifError = function(value) {
                    /// <summary><p>Tests if value is not a false value, throws if it is a true value. Useful when&#10;testing the first argument, <code>error</code> in callbacks.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="value"></param>
                    /// </signature>
                }
            };
            case "tty": return new     function tty() {
                /// <summary><p>The <code>tty</code> module houses the <code>tty.ReadStream</code> and <code>tty.WriteStream</code> classes. In&#10;most cases, you will not need to use this module directly.&#10;&#10;</p>&#10;<p>When node detects that it is being run inside a TTY context, then <code>process.stdin</code>&#10;will be a <code>tty.ReadStream</code> instance and <code>process.stdout</code> will be&#10;a <code>tty.WriteStream</code> instance. The preferred way to check if node is being run in&#10;a TTY context is to check <code>process.stdout.isTTY</code>:&#10;&#10;</p>&#10;<pre><code>$ node -p -e &quot;Boolean(process.stdout.isTTY)&quot;&#10;true&#10;$ node -p -e &quot;Boolean(process.stdout.isTTY)&quot; | cat&#10;false</code></pre>&#10;</summary>
                this.isatty = function(fd) {
                    /// <summary><p>Returns <code>true</code> or <code>false</code> depending on if the <code>fd</code> is associated with a&#10;terminal.&#10;&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="fd"></param>
                    /// </signature>
                }
                this.setRawMode = function(mode) {
                    /// <summary><p>Deprecated. Use <code>tty.ReadStream#setRawMode()</code>&#10;(i.e. <code>process.stdin.setRawMode()</code>) instead.&#10;&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="mode"></param>
                    /// </signature>
                }
                function _ReadStream() {
                    this.setRawMode = function(mode) {
                        /// <summary><p><code>mode</code> should be <code>true</code> or <code>false</code>. This sets the properties of the&#10;<code>tty.ReadStream</code> to act either as a raw device or default. <code>isRaw</code> will be set&#10;to the resulting mode.&#10;&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="mode"></param>
                        /// </signature>
                    }
                    /// <field name='isRaw'><p>A <code>Boolean</code> that is initialized to <code>false</code>. It represents the current &quot;raw&quot; state&#10;of the <code>tty.ReadStream</code> instance.&#10;&#10;</p>&#10;</field>
                    this.isRaw = true;
                }

                this.ReadStream = function() {
                    return new _ReadStream();
                }
                function _WriteStream() {
                    this.addListener = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// resize: <code>function () {}</code> ...&#10;
                        /// </summary>

                    }
                    this.once = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// resize: <code>function () {}</code> ...&#10;
                        /// </summary>

                    }
                    this.removeListener = function(event, listener) {
                        /// <summary>Supported Events: resize</summary>
                    }
                    this.removeAllListeners = function(event) {
                        /// <summary>Supported Events: resize</summary>
                    }
                    this.setMaxListeners = function(n) { }
                    this.listeners = function(event) {
                        /// <summary>Supported Events: resize</summary>
                    }
                    this.emit = function(event, arguments) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// resize: <code>function () {}</code> ...&#10;
                        /// </summary>

                    }
                    this.on = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// resize: <code>function () {}</code> ...&#10;
                        /// </summary>

                    }
                    /// <field name='columns'><p>A <code>Number</code> that gives the number of columns the TTY currently has. This property&#10;gets updated on &quot;resize&quot; events.&#10;&#10;</p>&#10;</field>
                    this.columns = 0;
                    /// <field name='rows'><p>A <code>Number</code> that gives the number of rows the TTY currently has. This property&#10;gets updated on &quot;resize&quot; events.&#10;&#10;</p>&#10;</field>
                    this.rows = 0;
                }

                this.WriteStream = function() {
                    return new _WriteStream();
                }
            };
            case "zlib": return new     function zlib() {
                /// <summary><p>You can access this module with:&#10;&#10;</p>&#10;<pre><code>var zlib = require(&#39;zlib&#39;);</code></pre>&#10;<p>This provides bindings to Gzip/Gunzip, Deflate/Inflate, and&#10;DeflateRaw/InflateRaw classes.  Each class takes the same options, and&#10;is a readable/writable Stream.&#10;&#10;</p>&#10;<h2>Examples</h2>&#10;<p>Compressing or decompressing a file can be done by piping an&#10;fs.ReadStream into a zlib stream, then into an fs.WriteStream.&#10;&#10;</p>&#10;<pre><code>var gzip = zlib.createGzip();&#10;var fs = require(&#39;fs&#39;);&#10;var inp = fs.createReadStream(&#39;input.txt&#39;);&#10;var out = fs.createWriteStream(&#39;input.txt.gz&#39;);&#10;&#10;inp.pipe(gzip).pipe(out);</code></pre>&#10;<p>Compressing or decompressing data in one step can be done by using&#10;the convenience methods.&#10;&#10;</p>&#10;<pre><code>var input = &#39;.................................&#39;;&#10;zlib.deflate(input, function(err, buffer) {&#10;  if (!err) {&#10;    console.log(buffer.toString(&#39;base64&#39;));&#10;  }&#10;});&#10;&#10;var buffer = new Buffer(&#39;eJzT0yMAAGTvBe8=&#39;, &#39;base64&#39;);&#10;zlib.unzip(buffer, function(err, buffer) {&#10;  if (!err) {&#10;    console.log(buffer.toString());&#10;  }&#10;});</code></pre>&#10;<p>To use this module in an HTTP client or server, use the&#10;<a href="http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.3">accept-encoding</a>&#10;on requests, and the&#10;<a href="http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.11">content-encoding</a>&#10;header on responses.&#10;&#10;</p>&#10;<p><strong>Note: these examples are drastically simplified to show&#10;the basic concept.</strong>  Zlib encoding can be expensive, and the results&#10;ought to be cached.  See <a href="#zlib_memory_usage_tuning">Memory Usage Tuning</a>&#10;below for more information on the speed/memory/compression&#10;tradeoffs involved in zlib usage.&#10;&#10;</p>&#10;<pre><code>// client request example&#10;var zlib = require(&#39;zlib&#39;);&#10;var http = require(&#39;http&#39;);&#10;var fs = require(&#39;fs&#39;);&#10;var request = http.get({ host: &#39;izs.me&#39;,&#10;                         path: &#39;/&#39;,&#10;                         port: 80,&#10;                         headers: { &#39;accept-encoding&#39;: &#39;gzip,deflate&#39; } });&#10;request.on(&#39;response&#39;, function(response) {&#10;  var output = fs.createWriteStream(&#39;izs.me_index.html&#39;);&#10;&#10;  switch (response.headers[&#39;content-encoding&#39;]) {&#10;    // or, just use zlib.createUnzip() to handle both cases&#10;    case &#39;gzip&#39;:&#10;      response.pipe(zlib.createGunzip()).pipe(output);&#10;      break;&#10;    case &#39;deflate&#39;:&#10;      response.pipe(zlib.createInflate()).pipe(output);&#10;      break;&#10;    default:&#10;      response.pipe(output);&#10;      break;&#10;  }&#10;});&#10;&#10;// server example&#10;// Running a gzip operation on every request is quite expensive.&#10;// It would be much more efficient to cache the compressed buffer.&#10;var zlib = require(&#39;zlib&#39;);&#10;var http = require(&#39;http&#39;);&#10;var fs = require(&#39;fs&#39;);&#10;http.createServer(function(request, response) {&#10;  var raw = fs.createReadStream(&#39;index.html&#39;);&#10;  var acceptEncoding = request.headers[&#39;accept-encoding&#39;];&#10;  if (!acceptEncoding) {&#10;    acceptEncoding = &#39;&#39;;&#10;  }&#10;&#10;  // Note: this is not a conformant accept-encoding parser.&#10;  // See http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.3&#10;  if (acceptEncoding.match(/\bdeflate\b/)) {&#10;    response.writeHead(200, { &#39;content-encoding&#39;: &#39;deflate&#39; });&#10;    raw.pipe(zlib.createDeflate()).pipe(response);&#10;  } else if (acceptEncoding.match(/\bgzip\b/)) {&#10;    response.writeHead(200, { &#39;content-encoding&#39;: &#39;gzip&#39; });&#10;    raw.pipe(zlib.createGzip()).pipe(response);&#10;  } else {&#10;    response.writeHead(200, {});&#10;    raw.pipe(response);&#10;  }&#10;}).listen(1337);</code></pre>&#10;</summary>
                this.createGzip = function(options) {
                    /// <summary><p>Returns a new <a href="#zlib_class_zlib_gzip">Gzip</a> object with an&#10;<a href="#zlib_options">options</a>.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="options"></param>
                    /// </signature>
                    return new this.Gzip();
                }
                this.createGunzip = function(options) {
                    /// <summary><p>Returns a new <a href="#zlib_class_zlib_gunzip">Gunzip</a> object with an&#10;<a href="#zlib_options">options</a>.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="options"></param>
                    /// </signature>
                    return new this.Gunzip();
                }
                this.createDeflate = function(options) {
                    /// <summary><p>Returns a new <a href="#zlib_class_zlib_deflate">Deflate</a> object with an&#10;<a href="#zlib_options">options</a>.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="options"></param>
                    /// </signature>
                    return new this.Deflate();
                }
                this.createInflate = function(options) {
                    /// <summary><p>Returns a new <a href="#zlib_class_zlib_inflate">Inflate</a> object with an&#10;<a href="#zlib_options">options</a>.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="options"></param>
                    /// </signature>
                    return new this.Inflate();
                }
                this.createDeflateRaw = function(options) {
                    /// <summary><p>Returns a new <a href="#zlib_class_zlib_deflateraw">DeflateRaw</a> object with an&#10;<a href="#zlib_options">options</a>.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="options"></param>
                    /// </signature>
                    return new this.DeflateRaw();
                }
                this.createInflateRaw = function(options) {
                    /// <summary><p>Returns a new <a href="#zlib_class_zlib_inflateraw">InflateRaw</a> object with an&#10;<a href="#zlib_options">options</a>.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="options"></param>
                    /// </signature>
                    return new this.InflateRaw();
                }
                this.createUnzip = function(options) {
                    /// <summary><p>Returns a new <a href="#zlib_class_zlib_unzip">Unzip</a> object with an&#10;<a href="#zlib_options">options</a>.&#10;&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="options"></param>
                    /// </signature>
                    return new this.Unzip();
                }
                this.deflate = function(buf, callback) {
                    /// <summary><p>Compress a string with Deflate.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="buf"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.deflateRaw = function(buf, callback) {
                    /// <summary><p>Compress a string with DeflateRaw.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="buf"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.gzip = function(buf, callback) {
                    /// <summary><p>Compress a string with Gzip.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="buf"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.gunzip = function(buf, callback) {
                    /// <summary><p>Decompress a raw Buffer with Gunzip.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="buf"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.inflate = function(buf, callback) {
                    /// <summary><p>Decompress a raw Buffer with Inflate.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="buf"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.inflateRaw = function(buf, callback) {
                    /// <summary><p>Decompress a raw Buffer with InflateRaw.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="buf"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.unzip = function(buf, callback) {
                    /// <summary><p>Decompress a raw Buffer with Unzip.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="buf"></param>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                function _Zlib() {
                    this.flush = function(callback) {
                        /// <summary><p>Flush pending data. Don&#39;t call this frivolously, premature flushes negatively&#10;impact the effectiveness of the compression algorithm.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="callback"></param>
                        /// </signature>
                    }
                    this.reset = function() {
                        /// <summary><p>Reset the compressor/decompressor to factory defaults. Only applicable to&#10;the inflate and deflate algorithms.&#10;&#10;</p>&#10;</summary>
                    }
                }

                this.Zlib = function() {
                    return new _Zlib();
                }
                function _Gzip() {
                }

                this.Gzip = function() {
                    return new _Gzip();
                }
                function _Gunzip() {
                }

                this.Gunzip = function() {
                    return new _Gunzip();
                }
                function _Deflate() {
                }

                this.Deflate = function() {
                    return new _Deflate();
                }
                function _Inflate() {
                }

                this.Inflate = function() {
                    return new _Inflate();
                }
                function _DeflateRaw() {
                }

                this.DeflateRaw = function() {
                    return new _DeflateRaw();
                }
                function _InflateRaw() {
                }

                this.InflateRaw = function() {
                    return new _InflateRaw();
                }
                function _Unzip() {
                }

                this.Unzip = function() {
                    return new _Unzip();
                }
            };
            case "os": return new     function os() {
                /// <summary><p>Provides a few basic operating-system related utility functions.&#10;&#10;</p>&#10;<p>Use <code>require(&#39;os&#39;)</code> to access this module.&#10;&#10;</p>&#10;</summary>
                this.tmpdir = function() {
                    /// <summary><p>Returns the operating system&#39;s default directory for temp files.&#10;&#10;</p>&#10;</summary>
                }
                this.endianness = function() {
                    /// <summary><p>Returns the endianness of the CPU. Possible values are <code>&quot;BE&quot;</code> or <code>&quot;LE&quot;</code>.&#10;&#10;</p>&#10;</summary>
                }
                this.hostname = function() {
                    /// <summary><p>Returns the hostname of the operating system.&#10;&#10;</p>&#10;</summary>
                }
                this.type = function() {
                    /// <summary><p>Returns the operating system name.&#10;&#10;</p>&#10;</summary>
                }
                this.platform = function() {
                    /// <summary><p>Returns the operating system platform.&#10;&#10;</p>&#10;</summary>
                }
                this.arch = function() {
                    /// <summary><p>Returns the operating system CPU architecture.&#10;&#10;</p>&#10;</summary>
                }
                this.release = function() {
                    /// <summary><p>Returns the operating system release.&#10;&#10;</p>&#10;</summary>
                }
                this.uptime = function() {
                    /// <summary><p>Returns the system uptime in seconds.&#10;&#10;</p>&#10;</summary>
                }
                this.loadavg = function() {
                    /// <summary><p>Returns an array containing the 1, 5, and 15 minute load averages.&#10;&#10;</p>&#10;</summary>
                }
                this.totalmem = function() {
                    /// <summary><p>Returns the total amount of system memory in bytes.&#10;&#10;</p>&#10;</summary>
                }
                this.freemem = function() {
                    /// <summary><p>Returns the amount of free system memory in bytes.&#10;&#10;</p>&#10;</summary>
                }
                this.cpus = function() {
                    /// <summary><p>Returns an array of objects containing information about each CPU/core&#10;installed: model, speed (in MHz), and times (an object containing the number of&#10;milliseconds the CPU/core spent in: user, nice, sys, idle, and irq).&#10;&#10;</p>&#10;<p>Example inspection of os.cpus:&#10;&#10;</p>&#10;<pre><code>[ { model: &#39;Intel(R) Core(TM) i7 CPU         860  @ 2.80GHz&#39;,&#10;    speed: 2926,&#10;    times:&#10;     { user: 252020,&#10;       nice: 0,&#10;       sys: 30340,&#10;       idle: 1070356870,&#10;       irq: 0 } },&#10;  { model: &#39;Intel(R) Core(TM) i7 CPU         860  @ 2.80GHz&#39;,&#10;    speed: 2926,&#10;    times:&#10;     { user: 306960,&#10;       nice: 0,&#10;       sys: 26980,&#10;       idle: 1071569080,&#10;       irq: 0 } },&#10;  { model: &#39;Intel(R) Core(TM) i7 CPU         860  @ 2.80GHz&#39;,&#10;    speed: 2926,&#10;    times:&#10;     { user: 248450,&#10;       nice: 0,&#10;       sys: 21750,&#10;       idle: 1070919370,&#10;       irq: 0 } },&#10;  { model: &#39;Intel(R) Core(TM) i7 CPU         860  @ 2.80GHz&#39;,&#10;    speed: 2926,&#10;    times:&#10;     { user: 256880,&#10;       nice: 0,&#10;       sys: 19430,&#10;       idle: 1070905480,&#10;       irq: 20 } },&#10;  { model: &#39;Intel(R) Core(TM) i7 CPU         860  @ 2.80GHz&#39;,&#10;    speed: 2926,&#10;    times:&#10;     { user: 511580,&#10;       nice: 20,&#10;       sys: 40900,&#10;       idle: 1070842510,&#10;       irq: 0 } },&#10;  { model: &#39;Intel(R) Core(TM) i7 CPU         860  @ 2.80GHz&#39;,&#10;    speed: 2926,&#10;    times:&#10;     { user: 291660,&#10;       nice: 0,&#10;       sys: 34360,&#10;       idle: 1070888000,&#10;       irq: 10 } },&#10;  { model: &#39;Intel(R) Core(TM) i7 CPU         860  @ 2.80GHz&#39;,&#10;    speed: 2926,&#10;    times:&#10;     { user: 308260,&#10;       nice: 0,&#10;       sys: 55410,&#10;       idle: 1071129970,&#10;       irq: 880 } },&#10;  { model: &#39;Intel(R) Core(TM) i7 CPU         860  @ 2.80GHz&#39;,&#10;    speed: 2926,&#10;    times:&#10;     { user: 266450,&#10;       nice: 1480,&#10;       sys: 34920,&#10;       idle: 1072572010,&#10;       irq: 30 } } ]</code></pre>&#10;</summary>
                }
                this.networkInterfaces = function() {
                    /// <summary><p>Get a list of network interfaces:&#10;&#10;</p>&#10;<pre><code>{ lo0: &#10;   [ { address: &#39;::1&#39;, family: &#39;IPv6&#39;, internal: true },&#10;     { address: &#39;fe80::1&#39;, family: &#39;IPv6&#39;, internal: true },&#10;     { address: &#39;127.0.0.1&#39;, family: &#39;IPv4&#39;, internal: true } ],&#10;  en1: &#10;   [ { address: &#39;fe80::cabc:c8ff:feef:f996&#39;, family: &#39;IPv6&#39;,&#10;       internal: false },&#10;     { address: &#39;10.0.1.123&#39;, family: &#39;IPv4&#39;, internal: false } ],&#10;  vmnet1: [ { address: &#39;10.99.99.254&#39;, family: &#39;IPv4&#39;, internal: false } ],&#10;  vmnet8: [ { address: &#39;10.88.88.1&#39;, family: &#39;IPv4&#39;, internal: false } ],&#10;  ppp0: [ { address: &#39;10.2.0.231&#39;, family: &#39;IPv4&#39;, internal: false } ] }</code></pre>&#10;</summary>
                }
                /// <field name='EOL'><p>A constant defining the appropriate End-of-line marker for the operating system.&#10;&#10;</p>&#10;</field>
                this.EOL = undefined;
            };
            case "cluster": return new     function cluster() {
                /// <summary><p>A single instance of Node runs in a single thread. To take advantage of&#10;multi-core systems the user will sometimes want to launch a cluster of Node&#10;processes to handle the load.&#10;&#10;</p>&#10;<p>The cluster module allows you to easily create a network of processes that&#10;all share server ports.&#10;&#10;</p>&#10;<pre><code>var cluster = require(&#39;cluster&#39;);&#10;var http = require(&#39;http&#39;);&#10;var numCPUs = require(&#39;os&#39;).cpus().length;&#10;&#10;if (cluster.isMaster) {&#10;  // Fork workers.&#10;  for (var i = 0; i &lt; numCPUs; i++) {&#10;    cluster.fork();&#10;  }&#10;&#10;  cluster.on(&#39;exit&#39;, function(worker, code, signal) {&#10;    console.log(&#39;worker &#39; + worker.process.pid + &#39; died&#39;);&#10;  });&#10;} else {&#10;  // Workers can share any TCP connection&#10;  // In this case its a HTTP server&#10;  http.createServer(function(req, res) {&#10;    res.writeHead(200);&#10;    res.end(&quot;hello world\n&quot;);&#10;  }).listen(8000);&#10;}</code></pre>&#10;<p>Running node will now share port 8000 between the workers:&#10;&#10;</p>&#10;<pre><code>% NODE_DEBUG=cluster node server.js&#10;23521,Master Worker 23524 online&#10;23521,Master Worker 23526 online&#10;23521,Master Worker 23523 online&#10;23521,Master Worker 23528 online</code></pre>&#10;<p>This feature was introduced recently, and may change in future versions.&#10;Please try it out and provide feedback.&#10;&#10;</p>&#10;<p>Also note that, on Windows, it is not yet possible to set up a named pipe&#10;server in a worker.&#10;&#10;</p>&#10;</summary>
                this.setupMaster = function(settings) {
                    /// <summary><p><code>setupMaster</code> is used to change the default &#39;fork&#39; behavior. The new settings&#10;are effective immediately and permanently, they cannot be changed later on.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>var cluster = require(&quot;cluster&quot;);&#10;cluster.setupMaster({&#10;  exec : &quot;worker.js&quot;,&#10;  args : [&quot;--use&quot;, &quot;https&quot;],&#10;  silent : true&#10;});&#10;cluster.fork();</code></pre>&#10;</summary>
                    /// <signature>
                    /// <param name="settings" type="Object"></param>
                    /// </signature>
                    /// <signature>
                    /// <param name="settings"></param>
                    /// </signature>
                }
                this.fork = function(env) {
                    /// <summary><p>Spawn a new worker process. This can only be called from the master process.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="env" type="Object">Key/value pairs to add to child process environment.</param>
                    /// <returns type="Worker"></returns>
                    /// </signature>
                    /// <signature>
                    /// <param name="env"></param>
                    /// </signature>
                }
                this.disconnect = function(callback) {
                    /// <summary><p>When calling this method, all workers will commit a graceful suicide. When they are&#10;disconnected all internal handlers will be closed, allowing the master process to&#10;die graceful if no other event is waiting.&#10;&#10;</p>&#10;<p>The method takes an optional callback argument which will be called when finished.&#10;&#10;</p>&#10;</summary>
                    /// <signature>
                    /// <param name="callback" type="Function">called when all workers are disconnected and handlers are closed</param>
                    /// </signature>
                    /// <signature>
                    /// <param name="callback"></param>
                    /// </signature>
                }
                this.addListener = function(event, listener) {

                    /// <summary>
                    /// Supported events: &#10;
                    /// fork: When a new worker is forked the cluster module will emit a &#39;fork&#39; event. ...&#10;
                    /// online: After forking a new worker, the worker should respond with a online message. ...&#10;
                    /// listening: When calling <code>listen()</code> from a worker, a &#39;listening&#39; event is automatically assigned ...&#10;
                    /// disconnect: When a workers IPC channel has disconnected this event is emitted. ...&#10;
                    /// exit: When any of the workers die the cluster module will emit the &#39;exit&#39; event. ...&#10;
                    /// setup: When the <code>.setupMaster()</code> function has been executed this event emits. ...&#10;
                    /// </summary>

                }
                this.once = function(event, listener) {

                    /// <summary>
                    /// Supported events: &#10;
                    /// fork: When a new worker is forked the cluster module will emit a &#39;fork&#39; event. ...&#10;
                    /// online: After forking a new worker, the worker should respond with a online message. ...&#10;
                    /// listening: When calling <code>listen()</code> from a worker, a &#39;listening&#39; event is automatically assigned ...&#10;
                    /// disconnect: When a workers IPC channel has disconnected this event is emitted. ...&#10;
                    /// exit: When any of the workers die the cluster module will emit the &#39;exit&#39; event. ...&#10;
                    /// setup: When the <code>.setupMaster()</code> function has been executed this event emits. ...&#10;
                    /// </summary>

                }
                this.removeListener = function(event, listener) {
                    /// <summary>Supported Events: fork, online, listening, disconnect, exit, setup</summary>
                }
                this.removeAllListeners = function(event) {
                    /// <summary>Supported Events: fork, online, listening, disconnect, exit, setup</summary>
                }
                this.setMaxListeners = function(n) { }
                this.listeners = function(event) {
                    /// <summary>Supported Events: fork, online, listening, disconnect, exit, setup</summary>
                }
                this.emit = function(event, arguments) {

                    /// <summary>
                    /// Supported events: &#10;
                    /// fork: When a new worker is forked the cluster module will emit a &#39;fork&#39; event. ...&#10;
                    /// online: After forking a new worker, the worker should respond with a online message. ...&#10;
                    /// listening: When calling <code>listen()</code> from a worker, a &#39;listening&#39; event is automatically assigned ...&#10;
                    /// disconnect: When a workers IPC channel has disconnected this event is emitted. ...&#10;
                    /// exit: When any of the workers die the cluster module will emit the &#39;exit&#39; event. ...&#10;
                    /// setup: When the <code>.setupMaster()</code> function has been executed this event emits. ...&#10;
                    /// </summary>

                }
                this.on = function(event, listener) {

                    /// <summary>
                    /// Supported events: &#10;
                    /// fork: When a new worker is forked the cluster module will emit a &#39;fork&#39; event. ...&#10;
                    /// online: After forking a new worker, the worker should respond with a online message. ...&#10;
                    /// listening: When calling <code>listen()</code> from a worker, a &#39;listening&#39; event is automatically assigned ...&#10;
                    /// disconnect: When a workers IPC channel has disconnected this event is emitted. ...&#10;
                    /// exit: When any of the workers die the cluster module will emit the &#39;exit&#39; event. ...&#10;
                    /// setup: When the <code>.setupMaster()</code> function has been executed this event emits. ...&#10;
                    /// </summary>

                }
                function _Worker() {
                    this.send = function(message, sendHandle) {
                        /// <summary><p>This function is equal to the send methods provided by&#10;<code>child_process.fork()</code>.  In the master you should use this function to&#10;send a message to a specific worker.  However in a worker you can also use&#10;<code>process.send(message)</code>, since this is the same function.&#10;&#10;</p>&#10;<p>This example will echo back all messages from the master:&#10;&#10;</p>&#10;<pre><code>if (cluster.isMaster) {&#10;  var worker = cluster.fork();&#10;  worker.send(&#39;hi there&#39;);&#10;&#10;} else if (cluster.isWorker) {&#10;  process.on(&#39;message&#39;, function(msg) {&#10;    process.send(msg);&#10;  });&#10;}</code></pre>&#10;</summary>
                        /// <signature>
                        /// <param name="message" type="Object"></param>
                        /// <param name="sendHandle" type="Handle"></param>
                        /// </signature>
                        /// <signature>
                        /// <param name="message"></param>
                        /// <param name="sendHandle"></param>
                        /// </signature>
                    }
                    this.kill = function(signal) {
                        /// <summary><p>This function will kill the worker, and inform the master to not spawn a&#10;new worker.  The boolean <code>suicide</code> lets you distinguish between voluntary&#10;and accidental exit.&#10;&#10;</p>&#10;<pre><code>cluster.on(&#39;exit&#39;, function(worker, code, signal) {&#10;  if (worker.suicide === true) {&#10;    console.log(&#39;Oh, it was just suicide\&#39; – no need to worry&#39;).&#10;  }&#10;});&#10;&#10;// kill worker&#10;worker.kill();</code></pre>&#10;<p>This method is aliased as <code>worker.destroy()</code> for backwards&#10;compatibility.&#10;&#10;</p>&#10;</summary>
                        /// <signature>
                        /// <param name="signal" type="String">Name of the kill signal to send to the worker process.</param>
                        /// </signature>
                        /// <signature>
                        /// <param name="signal"></param>
                        /// </signature>
                    }
                    this.disconnect = function() {
                        /// <summary><p>When calling this function the worker will no longer accept new connections, but&#10;they will be handled by any other listening worker. Existing connection will be&#10;allowed to exit as usual. When no more connections exist, the IPC channel to the worker&#10;will close allowing it to die graceful. When the IPC channel is closed the <code>disconnect</code>&#10;event will emit, this is then followed by the <code>exit</code> event, there is emitted when&#10;the worker finally die.&#10;&#10;</p>&#10;<p>Because there might be long living connections, it is useful to implement a timeout.&#10;This example ask the worker to disconnect and after 2 seconds it will destroy the&#10;server. An alternative would be to execute <code>worker.kill()</code> after 2 seconds, but&#10;that would normally not allow the worker to do any cleanup if needed.&#10;&#10;</p>&#10;<pre><code>if (cluster.isMaster) {&#10;  var worker = cluster.fork();&#10;  var timeout;&#10;&#10;  worker.on(&#39;listening&#39;, function(address) {&#10;    worker.disconnect();&#10;    timeout = setTimeout(function() {&#10;      worker.send(&#39;force kill&#39;);&#10;    }, 2000);&#10;  });&#10;&#10;  worker.on(&#39;disconnect&#39;, function() {&#10;    clearTimeout(timeout);&#10;  });&#10;&#10;} else if (cluster.isWorker) {&#10;  var net = require(&#39;net&#39;);&#10;  var server = net.createServer(function(socket) {&#10;    // connection never end&#10;  });&#10;&#10;  server.listen(8000);&#10;&#10;  server.on(&#39;close&#39;, function() {&#10;    // cleanup&#10;  });&#10;&#10;  process.on(&#39;message&#39;, function(msg) {&#10;    if (msg === &#39;force kill&#39;) {&#10;      server.close();&#10;    }&#10;  });&#10;}</code></pre>&#10;</summary>
                    }
                    this.addListener = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// message: This event is the same as the one provided by <code>child_process.fork()</code>. ...&#10;
                        /// online: Same as the <code>cluster.on(&#39;online&#39;)</code> event, but emits only when the state change ...&#10;
                        /// listening: Same as the <code>cluster.on(&#39;listening&#39;)</code> event, but emits only when the state change ...&#10;
                        /// disconnect: Same as the <code>cluster.on(&#39;disconnect&#39;)</code> event, but emits only when the state change ...&#10;
                        /// exit: Emitted by the individual worker instance, when the underlying child process ...&#10;
                        /// </summary>

                    }
                    this.once = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// message: This event is the same as the one provided by <code>child_process.fork()</code>. ...&#10;
                        /// online: Same as the <code>cluster.on(&#39;online&#39;)</code> event, but emits only when the state change ...&#10;
                        /// listening: Same as the <code>cluster.on(&#39;listening&#39;)</code> event, but emits only when the state change ...&#10;
                        /// disconnect: Same as the <code>cluster.on(&#39;disconnect&#39;)</code> event, but emits only when the state change ...&#10;
                        /// exit: Emitted by the individual worker instance, when the underlying child process ...&#10;
                        /// </summary>

                    }
                    this.removeListener = function(event, listener) {
                        /// <summary>Supported Events: message, online, listening, disconnect, exit</summary>
                    }
                    this.removeAllListeners = function(event) {
                        /// <summary>Supported Events: message, online, listening, disconnect, exit</summary>
                    }
                    this.setMaxListeners = function(n) { }
                    this.listeners = function(event) {
                        /// <summary>Supported Events: message, online, listening, disconnect, exit</summary>
                    }
                    this.emit = function(event, arguments) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// message: This event is the same as the one provided by <code>child_process.fork()</code>. ...&#10;
                        /// online: Same as the <code>cluster.on(&#39;online&#39;)</code> event, but emits only when the state change ...&#10;
                        /// listening: Same as the <code>cluster.on(&#39;listening&#39;)</code> event, but emits only when the state change ...&#10;
                        /// disconnect: Same as the <code>cluster.on(&#39;disconnect&#39;)</code> event, but emits only when the state change ...&#10;
                        /// exit: Emitted by the individual worker instance, when the underlying child process ...&#10;
                        /// </summary>

                    }
                    this.on = function(event, listener) {

                        /// <summary>
                        /// Supported events: &#10;
                        /// message: This event is the same as the one provided by <code>child_process.fork()</code>. ...&#10;
                        /// online: Same as the <code>cluster.on(&#39;online&#39;)</code> event, but emits only when the state change ...&#10;
                        /// listening: Same as the <code>cluster.on(&#39;listening&#39;)</code> event, but emits only when the state change ...&#10;
                        /// disconnect: Same as the <code>cluster.on(&#39;disconnect&#39;)</code> event, but emits only when the state change ...&#10;
                        /// exit: Emitted by the individual worker instance, when the underlying child process ...&#10;
                        /// </summary>

                    }
                    /// <field name='id'><p>Each new worker is given its own unique id, this id is stored in the&#10;<code>id</code>.&#10;&#10;</p>&#10;<p>While a worker is alive, this is the key that indexes it in&#10;cluster.workers&#10;&#10;</p>&#10;</field>
                    this.id = undefined;
                    /// <field name='process'><p>All workers are created using <code>child_process.fork()</code>, the returned object&#10;from this function is stored in process.&#10;&#10;</p>&#10;<p>See: <a href="child_process.html">Child Process module</a>&#10;&#10;</p>&#10;</field>
                    this.process = undefined;
                    /// <field name='suicide'><p>This property is a boolean. It is set when a worker dies after calling&#10;<code>.kill()</code> or immediately after calling the <code>.disconnect()</code> method.&#10;Until then it is <code>undefined</code>.&#10;&#10;</p>&#10;</field>
                    this.suicide = undefined;
                }

                this.Worker = function() {
                    return new _Worker();
                }
                /// <field name='settings'><p>All settings set by the <code>.setupMaster</code> is stored in this settings object.&#10;This object is not supposed to be changed or set manually, by you.&#10;&#10;</p>&#10;</field>
                this.settings = undefined;
                /// <field name='isMaster'><p>True if the process is a master. This is determined&#10;by the <code>process.env.NODE_UNIQUE_ID</code>. If <code>process.env.NODE_UNIQUE_ID</code> is&#10;undefined, then <code>isMaster</code> is <code>true</code>.&#10;&#10;</p>&#10;</field>
                this.isMaster = undefined;
                /// <field name='isWorker'><p>This boolean flag is true if the process is a worker forked from a master.&#10;If the <code>process.env.NODE_UNIQUE_ID</code> is set to a value, then&#10;<code>isWorker</code> is <code>true</code>.&#10;&#10;</p>&#10;</field>
                this.isWorker = undefined;
                /// <field name='worker'><p>A reference to the current worker object. Not available in the master process.&#10;&#10;</p>&#10;<pre><code>var cluster = require(&#39;cluster&#39;);&#10;&#10;if (cluster.isMaster) {&#10;  console.log(&#39;I am master&#39;);&#10;  cluster.fork();&#10;  cluster.fork();&#10;} else if (cluster.isWorker) {&#10;  console.log(&#39;I am worker #&#39; + cluster.worker.id);&#10;}</code></pre>&#10;</field>
                this.worker = undefined;
                /// <field name='workers'><p>A hash that stores the active worker objects, keyed by <code>id</code> field. Makes it&#10;easy to loop through all the workers. It is only available in the master&#10;process.&#10;&#10;</p>&#10;<pre><code>// Go through all workers&#10;function eachWorker(callback) {&#10;  for (var id in cluster.workers) {&#10;    callback(cluster.workers[id]);&#10;  }&#10;}&#10;eachWorker(function(worker) {&#10;  worker.send(&#39;big announcement to all workers&#39;);&#10;});</code></pre>&#10;<p>Should you wish to reference a worker over a communication channel, using&#10;the worker&#39;s unique id is the easiest way to find the worker.&#10;&#10;</p>&#10;<pre><code>socket.on(&#39;data&#39;, function(id) {&#10;  var worker = cluster.workers[id];&#10;});</code></pre>&#10;</field>
                this.workers = undefined;
            };
        }
    }
    var f = function(module) { 
        module = module.replace(/\\/g, '/');
        if(require_count++ >= 50) {
            require_count = 0;
            intellisense.progress();
        }
        var result = cache[module];
        if(typeof result !== 'undefined') {
            if(result === null) {
                // we lazily create only the modules which are actually used
                cache[module] = result = make_module(module);
            }
            return result;
        }
        // value not cached, see if we can look it up
        try { 
            var __prevFilename = __filename;
            var __prevDirname = __dirname;
            var __prevMaxRequireDepth = max_require_depth;
            require_depth++;
            try {
                // **NTVS** INSERT USER MODULE SWITCH HERE **NTVS**
            } finally {
                require_depth--;
            }
        } finally {
            __filename = __prevFilename;
            __dirname = __prevDirname;
            max_require_depth = __prevMaxRequireDepth;
        }
    }
    o.__proto__ = f.__proto__;
    f.__proto__ = o;
    return f;
}();var console = new function __console() {
    /// <summary><p>For printing to stdout and stderr.  Similar to the console object functions&#10;provided by most web browsers, here the output is sent to stdout or stderr.&#10;&#10;</p>&#10;<p>The console functions are synchronous when the destination is a terminal or&#10;a file (to avoid lost messages in case of premature exit) and asynchronous&#10;when it&#39;s a pipe (to avoid blocking for long periods of time).&#10;&#10;</p>&#10;<p>That is, in the following example, stdout is non-blocking while stderr&#10;is blocking:&#10;&#10;</p>&#10;<pre><code>$ node script.js 2&gt; error.log | tee info.log</code></pre>&#10;<p>In daily use, the blocking/non-blocking dichotomy is not something you&#10;should worry about unless you log huge amounts of data.&#10;&#10;&#10;</p>&#10;</summary>
    this.log = function(data) {
        /// <summary><p>Prints to stdout with newline. This function can take multiple arguments in a&#10;<code>printf()</code>-like way. Example:&#10;&#10;</p>&#10;<pre><code>console.log(&#39;count: %d&#39;, count);</code></pre>&#10;<p>If formatting elements are not found in the first string then <code>util.inspect</code>&#10;is used on each argument.  See [util.format()][] for more information.&#10;&#10;</p>&#10;</summary>
        /// <signature>
        /// <param name="data"></param>
        /// <param name="..."></param>
        /// </signature>
    }
    this.info = function(data) {
        /// <summary><p>Same as <code>console.log</code>.&#10;&#10;</p>&#10;</summary>
        /// <signature>
        /// <param name="data"></param>
        /// <param name="..."></param>
        /// </signature>
    }
    this.error = function(data) {
        /// <summary><p>Same as <code>console.log</code> but prints to stderr.&#10;&#10;</p>&#10;</summary>
        /// <signature>
        /// <param name="data"></param>
        /// <param name="..."></param>
        /// </signature>
    }
    this.warn = function(data) {
        /// <summary><p>Same as <code>console.error</code>.&#10;&#10;</p>&#10;</summary>
        /// <signature>
        /// <param name="data"></param>
        /// <param name="..."></param>
        /// </signature>
    }
    this.dir = function(obj) {
        /// <summary><p>Uses <code>util.inspect</code> on <code>obj</code> and prints resulting string to stdout.&#10;&#10;</p>&#10;</summary>
        /// <signature>
        /// <param name="obj"></param>
        /// </signature>
    }
    this.time = function(label) {
        /// <summary><p>Mark a time.&#10;&#10;</p>&#10;</summary>
        /// <signature>
        /// <param name="label"></param>
        /// </signature>
    }
    this.timeEnd = function(label) {
        /// <summary><p>Finish timer, record output. Example:&#10;&#10;</p>&#10;<pre><code>console.time(&#39;100-elements&#39;);&#10;for (var i = 0; i &lt; 100; i++) {&#10;  ;&#10;}&#10;console.timeEnd(&#39;100-elements&#39;);</code></pre>&#10;</summary>
        /// <signature>
        /// <param name="label"></param>
        /// </signature>
    }
    this.trace = function(label) {
        /// <summary><p>Print a stack trace to stderr of the current position.&#10;&#10;</p>&#10;</summary>
        /// <signature>
        /// <param name="label"></param>
        /// </signature>
    }
    this.assert = function(expression, message) {
        /// <summary><p>Same as [assert.ok()][] where if the <code>expression</code> evaluates as <code>false</code> throw an&#10;AssertionError with <code>message</code>.&#10;&#10;</p>&#10;</summary>
        /// <signature>
        /// <param name="expression"></param>
        /// <param name="message"></param>
        /// </signature>
    }
};var process = new function __process() {
    /// <summary><p>The <code>process</code> object is a global object and can be accessed from anywhere.&#10;It is an instance of [EventEmitter][].&#10;&#10;&#10;</p>&#10;</summary>
    this.abort = function() {
        /// <summary><p>This causes node to emit an abort. This will cause node to exit and&#10;generate a core file.&#10;&#10;</p>&#10;</summary>
    }
    this.chdir = function(directory) {
        /// <summary><p>Changes the current working directory of the process or throws an exception if that fails.&#10;&#10;</p>&#10;<pre><code>console.log(&#39;Starting directory: &#39; + process.cwd());&#10;try {&#10;  process.chdir(&#39;/tmp&#39;);&#10;  console.log(&#39;New directory: &#39; + process.cwd());&#10;}&#10;catch (err) {&#10;  console.log(&#39;chdir: &#39; + err);&#10;}</code></pre>&#10;</summary>
        /// <signature>
        /// <param name="directory"></param>
        /// </signature>
    }
    this.cwd = function() {
        /// <summary><p>Returns the current working directory of the process.&#10;&#10;</p>&#10;<pre><code>console.log(&#39;Current directory: &#39; + process.cwd());</code></pre>&#10;</summary>
    }
    this.exit = function(code) {
        /// <summary><p>Ends the process with the specified <code>code</code>.  If omitted, exit uses the&#10;&#39;success&#39; code <code>0</code>.&#10;&#10;</p>&#10;<p>To exit with a &#39;failure&#39; code:&#10;&#10;</p>&#10;<pre><code>process.exit(1);</code></pre>&#10;<p>The shell that executed node should see the exit code as 1.&#10;&#10;&#10;</p>&#10;</summary>
        /// <signature>
        /// <param name="code"></param>
        /// </signature>
    }
    this.getgid = function() {
        /// <summary><p>Note: this function is only available on POSIX platforms (i.e. not Windows)&#10;&#10;</p>&#10;<p>Gets the group identity of the process. (See getgid(2).)&#10;This is the numerical group id, not the group name.&#10;&#10;</p>&#10;<pre><code>if (process.getgid) {&#10;  console.log(&#39;Current gid: &#39; + process.getgid());&#10;}</code></pre>&#10;</summary>
    }
    this.setgid = function(id) {
        /// <summary><p>Note: this function is only available on POSIX platforms (i.e. not Windows)&#10;&#10;</p>&#10;<p>Sets the group identity of the process. (See setgid(2).)  This accepts either&#10;a numerical ID or a groupname string. If a groupname is specified, this method&#10;blocks while resolving it to a numerical ID.&#10;&#10;</p>&#10;<pre><code>if (process.getgid &amp;&amp; process.setgid) {&#10;  console.log(&#39;Current gid: &#39; + process.getgid());&#10;  try {&#10;    process.setgid(501);&#10;    console.log(&#39;New gid: &#39; + process.getgid());&#10;  }&#10;  catch (err) {&#10;    console.log(&#39;Failed to set gid: &#39; + err);&#10;  }&#10;}</code></pre>&#10;</summary>
        /// <signature>
        /// <param name="id"></param>
        /// </signature>
    }
    this.getuid = function() {
        /// <summary><p>Note: this function is only available on POSIX platforms (i.e. not Windows)&#10;&#10;</p>&#10;<p>Gets the user identity of the process. (See getuid(2).)&#10;This is the numerical userid, not the username.&#10;&#10;</p>&#10;<pre><code>if (process.getuid) {&#10;  console.log(&#39;Current uid: &#39; + process.getuid());&#10;}</code></pre>&#10;</summary>
    }
    this.setuid = function(id) {
        /// <summary><p>Note: this function is only available on POSIX platforms (i.e. not Windows)&#10;&#10;</p>&#10;<p>Sets the user identity of the process. (See setuid(2).)  This accepts either&#10;a numerical ID or a username string.  If a username is specified, this method&#10;blocks while resolving it to a numerical ID.&#10;&#10;</p>&#10;<pre><code>if (process.getuid &amp;&amp; process.setuid) {&#10;  console.log(&#39;Current uid: &#39; + process.getuid());&#10;  try {&#10;    process.setuid(501);&#10;    console.log(&#39;New uid: &#39; + process.getuid());&#10;  }&#10;  catch (err) {&#10;    console.log(&#39;Failed to set uid: &#39; + err);&#10;  }&#10;}</code></pre>&#10;</summary>
        /// <signature>
        /// <param name="id"></param>
        /// </signature>
    }
    this.getgroups = function() {
        /// <summary><p>Note: this function is only available on POSIX platforms (i.e. not Windows)&#10;&#10;</p>&#10;<p>Returns an array with the supplementary group IDs. POSIX leaves it unspecified&#10;if the effective group ID is included but node.js ensures it always is.&#10;&#10;&#10;</p>&#10;</summary>
    }
    this.setgroups = function(groups) {
        /// <summary><p>Note: this function is only available on POSIX platforms (i.e. not Windows)&#10;&#10;</p>&#10;<p>Sets the supplementary group IDs. This is a privileged operation, meaning you&#10;need to be root or have the CAP_SETGID capability.&#10;&#10;</p>&#10;<p>The list can contain group IDs, group names or both.&#10;&#10;&#10;</p>&#10;</summary>
        /// <signature>
        /// <param name="groups"></param>
        /// </signature>
    }
    this.initgroups = function(user, extra_group) {
        /// <summary><p>Note: this function is only available on POSIX platforms (i.e. not Windows)&#10;&#10;</p>&#10;<p>Reads /etc/group and initializes the group access list, using all groups of&#10;which the user is a member. This is a privileged operation, meaning you need&#10;to be root or have the CAP_SETGID capability.&#10;&#10;</p>&#10;<p><code>user</code> is a user name or user ID. <code>extra_group</code> is a group name or group ID.&#10;&#10;</p>&#10;<p>Some care needs to be taken when dropping privileges. Example:&#10;&#10;</p>&#10;<pre><code>console.log(process.getgroups());         // [ 0 ]&#10;process.initgroups(&#39;bnoordhuis&#39;, 1000);   // switch user&#10;console.log(process.getgroups());         // [ 27, 30, 46, 1000, 0 ]&#10;process.setgid(1000);                     // drop root gid&#10;console.log(process.getgroups());         // [ 27, 30, 46, 1000 ]</code></pre>&#10;</summary>
        /// <signature>
        /// <param name="user"></param>
        /// <param name="extra_group"></param>
        /// </signature>
    }
    this.kill = function(pid, signal) {
        /// <summary><p>Send a signal to a process. <code>pid</code> is the process id and <code>signal</code> is the&#10;string describing the signal to send.  Signal names are strings like&#10;&#39;SIGINT&#39; or &#39;SIGHUP&#39;.  If omitted, the signal will be &#39;SIGTERM&#39;.&#10;See kill(2) for more information.&#10;&#10;</p>&#10;<p>Note that just because the name of this function is <code>process.kill</code>, it is&#10;really just a signal sender, like the <code>kill</code> system call.  The signal sent&#10;may do something other than kill the target process.&#10;&#10;</p>&#10;<p>Example of sending a signal to yourself:&#10;&#10;</p>&#10;<pre><code>process.on(&#39;SIGHUP&#39;, function() {&#10;  console.log(&#39;Got SIGHUP signal.&#39;);&#10;});&#10;&#10;setTimeout(function() {&#10;  console.log(&#39;Exiting.&#39;);&#10;  process.exit(0);&#10;}, 100);&#10;&#10;process.kill(process.pid, &#39;SIGHUP&#39;);</code></pre>&#10;<p>Note: SIGUSR1 is reserved by node.js.  It can be used to kickstart the&#10;debugger.&#10;&#10;</p>&#10;</summary>
        /// <signature>
        /// <param name="pid"></param>
        /// <param name="signal"></param>
        /// </signature>
    }
    this.memoryUsage = function() {
        /// <summary><p>Returns an object describing the memory usage of the Node process&#10;measured in bytes.&#10;&#10;</p>&#10;<pre><code>var util = require(&#39;util&#39;);&#10;&#10;console.log(util.inspect(process.memoryUsage()));</code></pre>&#10;<p>This will generate:&#10;&#10;</p>&#10;<pre><code>{ rss: 4935680,&#10;  heapTotal: 1826816,&#10;  heapUsed: 650472 }</code></pre>&#10;<p><code>heapTotal</code> and <code>heapUsed</code> refer to V8&#39;s memory usage.&#10;&#10;&#10;</p>&#10;</summary>
    }
    this.nextTick = function(callback) {
        /// <summary><p>On the next loop around the event loop call this callback.&#10;This is <em>not</em> a simple alias to <code>setTimeout(fn, 0)</code>, it&#39;s much more&#10;efficient.  It typically runs before any other I/O events fire, but there&#10;are some exceptions.  See <code>process.maxTickDepth</code> below.&#10;&#10;</p>&#10;<pre><code>process.nextTick(function() {&#10;  console.log(&#39;nextTick callback&#39;);&#10;});</code></pre>&#10;<p>This is important in developing APIs where you want to give the user the&#10;chance to assign event handlers after an object has been constructed,&#10;but before any I/O has occurred.&#10;&#10;</p>&#10;<pre><code>function MyThing(options) {&#10;  this.setupOptions(options);&#10;&#10;  process.nextTick(function() {&#10;    this.startDoingStuff();&#10;  }.bind(this));&#10;}&#10;&#10;var thing = new MyThing();&#10;thing.getReadyForStuff();&#10;&#10;// thing.startDoingStuff() gets called now, not before.</code></pre>&#10;<p>It is very important for APIs to be either 100% synchronous or 100%&#10;asynchronous.  Consider this example:&#10;&#10;</p>&#10;<pre><code>// WARNING!  DO NOT USE!  BAD UNSAFE HAZARD!&#10;function maybeSync(arg, cb) {&#10;  if (arg) {&#10;    cb();&#10;    return;&#10;  }&#10;&#10;  fs.stat(&#39;file&#39;, cb);&#10;}</code></pre>&#10;<p>This API is hazardous.  If you do this:&#10;&#10;</p>&#10;<pre><code>maybeSync(true, function() {&#10;  foo();&#10;});&#10;bar();</code></pre>&#10;<p>then it&#39;s not clear whether <code>foo()</code> or <code>bar()</code> will be called first.&#10;&#10;</p>&#10;<p>This approach is much better:&#10;&#10;</p>&#10;<pre><code>function definitelyAsync(arg, cb) {&#10;  if (arg) {&#10;    process.nextTick(cb);&#10;    return;&#10;  }&#10;&#10;  fs.stat(&#39;file&#39;, cb);&#10;}</code></pre>&#10;</summary>
        /// <signature>
        /// <param name="callback"></param>
        /// </signature>
    }
    this.umask = function(mask) {
        /// <summary><p>Sets or reads the process&#39;s file mode creation mask. Child processes inherit&#10;the mask from the parent process. Returns the old mask if <code>mask</code> argument is&#10;given, otherwise returns the current mask.&#10;&#10;</p>&#10;<pre><code>var oldmask, newmask = 0644;&#10;&#10;oldmask = process.umask(newmask);&#10;console.log(&#39;Changed umask from: &#39; + oldmask.toString(8) +&#10;            &#39; to &#39; + newmask.toString(8));</code></pre>&#10;</summary>
        /// <signature>
        /// <param name="mask"></param>
        /// </signature>
    }
    this.uptime = function() {
        /// <summary><p>Number of seconds Node has been running.&#10;&#10;&#10;</p>&#10;</summary>
    }
    this.hrtime = function() {
        /// <summary><p>Returns the current high-resolution real time in a <code>[seconds, nanoseconds]</code>&#10;tuple Array. It is relative to an arbitrary time in the past. It is not&#10;related to the time of day and therefore not subject to clock drift. The&#10;primary use is for measuring performance between intervals.&#10;&#10;</p>&#10;<p>You may pass in the result of a previous call to <code>process.hrtime()</code> to get&#10;a diff reading, useful for benchmarks and measuring intervals:&#10;&#10;</p>&#10;<pre><code>var time = process.hrtime();&#10;// [ 1800216, 25 ]&#10;&#10;setTimeout(function() {&#10;  var diff = process.hrtime(time);&#10;  // [ 1, 552 ]&#10;&#10;  console.log(&#39;benchmark took %d nanoseconds&#39;, diff[0] * 1e9 + diff[1]);&#10;  // benchmark took 1000000527 nanoseconds&#10;}, 1000);</code></pre>&#10;</summary>
    }
    this.addListener = function(event, listener) {

        /// <summary>
        /// Supported events: &#10;
        /// exit: Emitted when the process is about to exit.  This is a good hook to perform ...&#10;
        /// uncaughtException: Emitted when an exception bubbles all the way back to the event loop. If a ...&#10;
        /// </summary>

    }
    this.once = function(event, listener) {

        /// <summary>
        /// Supported events: &#10;
        /// exit: Emitted when the process is about to exit.  This is a good hook to perform ...&#10;
        /// uncaughtException: Emitted when an exception bubbles all the way back to the event loop. If a ...&#10;
        /// </summary>

    }
    this.removeListener = function(event, listener) {
        /// <summary>Supported Events: exit, uncaughtException</summary>
    }
    this.removeAllListeners = function(event) {
        /// <summary>Supported Events: exit, uncaughtException</summary>
    }
    this.setMaxListeners = function(n) { }
    this.listeners = function(event) {
        /// <summary>Supported Events: exit, uncaughtException</summary>
    }
    this.emit = function(event, arguments) {

        /// <summary>
        /// Supported events: &#10;
        /// exit: Emitted when the process is about to exit.  This is a good hook to perform ...&#10;
        /// uncaughtException: Emitted when an exception bubbles all the way back to the event loop. If a ...&#10;
        /// </summary>

    }
    this.on = function(event, listener) {

        /// <summary>
        /// Supported events: &#10;
        /// exit: Emitted when the process is about to exit.  This is a good hook to perform ...&#10;
        /// uncaughtException: Emitted when an exception bubbles all the way back to the event loop. If a ...&#10;
        /// </summary>

    }
    /// <field name='stdout'><p>A <code>Writable Stream</code> to <code>stdout</code>.&#10;&#10;</p>&#10;<p>Example: the definition of <code>console.log</code>&#10;&#10;</p>&#10;<pre><code>console.log = function(d) {&#10;  process.stdout.write(d + &#39;\n&#39;);&#10;};</code></pre>&#10;<p><code>process.stderr</code> and <code>process.stdout</code> are unlike other streams in Node in&#10;that writes to them are usually blocking.  They are blocking in the case&#10;that they refer to regular files or TTY file descriptors. In the case they&#10;refer to pipes, they are non-blocking like other streams.&#10;&#10;</p>&#10;<p>To check if Node is being run in a TTY context, read the <code>isTTY</code> property&#10;on <code>process.stderr</code>, <code>process.stdout</code>, or <code>process.stdin</code>:&#10;&#10;</p>&#10;<pre><code>$ node -p &quot;Boolean(process.stdin.isTTY)&quot;&#10;true&#10;$ echo &quot;foo&quot; | node -p &quot;Boolean(process.stdin.isTTY)&quot;&#10;false&#10;&#10;$ node -p &quot;Boolean(process.stdout.isTTY)&quot;&#10;true&#10;$ node -p &quot;Boolean(process.stdout.isTTY)&quot; | cat&#10;false</code></pre>&#10;<p>See <a href="tty.html#tty_tty">the tty docs</a> for more information.&#10;&#10;</p>&#10;</field>
    this.stdout = require('stream').Writable();
    /// <field name='stderr'><p>A writable stream to stderr.&#10;&#10;</p>&#10;<p><code>process.stderr</code> and <code>process.stdout</code> are unlike other streams in Node in&#10;that writes to them are usually blocking.  They are blocking in the case&#10;that they refer to regular files or TTY file descriptors. In the case they&#10;refer to pipes, they are non-blocking like other streams.&#10;&#10;&#10;</p>&#10;</field>
    this.stderr = require('stream').Writable();
    /// <field name='stdin'><p>A <code>Readable Stream</code> for stdin. The stdin stream is paused by default, so one&#10;must call <code>process.stdin.resume()</code> to read from it.&#10;&#10;</p>&#10;<p>Example of opening standard input and listening for both events:&#10;&#10;</p>&#10;<pre><code>process.stdin.resume();&#10;process.stdin.setEncoding(&#39;utf8&#39;);&#10;&#10;process.stdin.on(&#39;data&#39;, function(chunk) {&#10;  process.stdout.write(&#39;data: &#39; + chunk);&#10;});&#10;&#10;process.stdin.on(&#39;end&#39;, function() {&#10;  process.stdout.write(&#39;end&#39;);&#10;});</code></pre>&#10;</field>
    this.stdin = require('stream').Readable();
    /// <field name='argv'><p>An array containing the command line arguments.  The first element will be&#10;&#39;node&#39;, the second element will be the name of the JavaScript file.  The&#10;next elements will be any additional command line arguments.&#10;&#10;</p>&#10;<pre><code>// print process.argv&#10;process.argv.forEach(function(val, index, array) {&#10;  console.log(index + &#39;: &#39; + val);&#10;});</code></pre>&#10;<p>This will generate:&#10;&#10;</p>&#10;<pre><code>$ node process-2.js one two=three four&#10;0: node&#10;1: /Users/mjr/work/node/process-2.js&#10;2: one&#10;3: two=three&#10;4: four</code></pre>&#10;</field>
    this.argv = [ 'node.exe' ];
    /// <field name='execPath'><p>This is the absolute pathname of the executable that started the process.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>/usr/local/bin/node</code></pre>&#10;</field>
    this.execPath = undefined;
    /// <field name='execArgv'><p>This is the set of node-specific command line options from the&#10;executable that started the process.  These options do not show up in&#10;<code>process.argv</code>, and do not include the node executable, the name of&#10;the script, or any options following the script name. These options&#10;are useful in order to spawn child processes with the same execution&#10;environment as the parent.&#10;&#10;</p>&#10;<p>Example:&#10;&#10;</p>&#10;<pre><code>$ node --harmony script.js --version</code></pre>&#10;<p>results in process.execArgv:&#10;&#10;</p>&#10;<pre><code>[&#39;--harmony&#39;]</code></pre>&#10;<p>and process.argv:&#10;&#10;</p>&#10;<pre><code>[&#39;/usr/local/bin/node&#39;, &#39;script.js&#39;, &#39;--version&#39;]</code></pre>&#10;</field>
    this.execArgv = undefined;
    /// <field name='env'><p>An object containing the user environment. See environ(7).&#10;&#10;&#10;</p>&#10;</field>
    this.env = {};
    /// <field name='version'><p>A compiled-in property that exposes <code>NODE_VERSION</code>.&#10;&#10;</p>&#10;<pre><code>console.log(&#39;Version: &#39; + process.version);</code></pre>&#10;</field>
    this.version = undefined;
    /// <field name='versions'><p>A property exposing version strings of node and its dependencies.&#10;&#10;</p>&#10;<pre><code>console.log(process.versions);</code></pre>&#10;<p>Will print something like:&#10;&#10;</p>&#10;<pre><code>{ http_parser: &#39;1.0&#39;,&#10;  node: &#39;0.10.4&#39;,&#10;  v8: &#39;3.14.5.8&#39;,&#10;  ares: &#39;1.9.0-DEV&#39;,&#10;  uv: &#39;0.10.3&#39;,&#10;  zlib: &#39;1.2.3&#39;,&#10;  modules: &#39;11&#39;,&#10;  openssl: &#39;1.0.1e&#39; }</code></pre>&#10;</field>
    this.versions = {node: '0.10.0', v8: '3.14.5.8'};
    /// <field name='config'><p>An Object containing the JavaScript representation of the configure options&#10;that were used to compile the current node executable. This is the same as&#10;the &quot;config.gypi&quot; file that was produced when running the <code>./configure</code> script.&#10;&#10;</p>&#10;<p>An example of the possible output looks like:&#10;&#10;</p>&#10;<pre><code>{ target_defaults:&#10;   { cflags: [],&#10;     default_configuration: &#39;Release&#39;,&#10;     defines: [],&#10;     include_dirs: [],&#10;     libraries: [] },&#10;  variables:&#10;   { host_arch: &#39;x64&#39;,&#10;     node_install_npm: &#39;true&#39;,&#10;     node_prefix: &#39;&#39;,&#10;     node_shared_cares: &#39;false&#39;,&#10;     node_shared_http_parser: &#39;false&#39;,&#10;     node_shared_libuv: &#39;false&#39;,&#10;     node_shared_v8: &#39;false&#39;,&#10;     node_shared_zlib: &#39;false&#39;,&#10;     node_use_dtrace: &#39;false&#39;,&#10;     node_use_openssl: &#39;true&#39;,&#10;     node_shared_openssl: &#39;false&#39;,&#10;     strict_aliasing: &#39;true&#39;,&#10;     target_arch: &#39;x64&#39;,&#10;     v8_use_snapshot: &#39;true&#39; } }</code></pre>&#10;</field>
    this.config = undefined;
    /// <field name='pid'><p>The PID of the process.&#10;&#10;</p>&#10;<pre><code>console.log(&#39;This process is pid &#39; + process.pid);</code></pre>&#10;</field>
    this.pid = 0;
    /// <field name='title'><p>Getter/setter to set what is displayed in &#39;ps&#39;.&#10;&#10;</p>&#10;<p>When used as a setter, the maximum length is platform-specific and probably&#10;short.&#10;&#10;</p>&#10;<p>On Linux and OS X, it&#39;s limited to the size of the binary name plus the&#10;length of the command line arguments because it overwrites the argv memory.&#10;&#10;</p>&#10;<p>v0.8 allowed for longer process title strings by also overwriting the environ&#10;memory but that was potentially insecure/confusing in some (rather obscure)&#10;cases.&#10;&#10;&#10;</p>&#10;</field>
    this.title = '';
    /// <field name='arch'><p>What processor architecture you&#39;re running on: <code>&#39;arm&#39;</code>, <code>&#39;ia32&#39;</code>, or <code>&#39;x64&#39;</code>.&#10;&#10;</p>&#10;<pre><code>console.log(&#39;This processor architecture is &#39; + process.arch);</code></pre>&#10;</field>
    this.arch = undefined;
    /// <field name='platform'><p>What platform you&#39;re running on:&#10;<code>&#39;darwin&#39;</code>, <code>&#39;freebsd&#39;</code>, <code>&#39;linux&#39;</code>, <code>&#39;sunos&#39;</code> or <code>&#39;win32&#39;</code>&#10;&#10;</p>&#10;<pre><code>console.log(&#39;This platform is &#39; + process.platform);</code></pre>&#10;</field>
    this.platform = 'win32';
    /// <field name='maxTickDepth'><p>Callbacks passed to <code>process.nextTick</code> will <em>usually</em> be called at the&#10;end of the current flow of execution, and are thus approximately as fast&#10;as calling a function synchronously.  Left unchecked, this would starve&#10;the event loop, preventing any I/O from occurring.&#10;&#10;</p>&#10;<p>Consider this code:&#10;&#10;</p>&#10;<pre><code>process.nextTick(function foo() {&#10;  process.nextTick(foo);&#10;});</code></pre>&#10;<p>In order to avoid the situation where Node is blocked by an infinite&#10;loop of recursive series of nextTick calls, it defers to allow some I/O&#10;to be done every so often.&#10;&#10;</p>&#10;<p>The <code>process.maxTickDepth</code> value is the maximum depth of&#10;nextTick-calling nextTick-callbacks that will be evaluated before&#10;allowing other forms of I/O to occur.&#10;&#10;</p>&#10;</field>
    this.maxTickDepth = 1000;
};