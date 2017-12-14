Object.defineProperty(window, '__line', {
    get: function () {
        return __stack[__stack.length - 2].getLineNumber();
    }
});

var DebugHelper = {

    // This should only be called when an error or warning occurs ( or any consolemessage that isn't called from console.log() )

	notifyConsoleMessage: function(formname, eventname, linenumber, message, sourcefile, responseType, messageType) {
        App.invokeOnFormAsync("debug", "addConsoleMessageToList('" + formname + "', '" + eventname + "', '" + linenumber + "', '" + message.replaceAll("\\", "\\\\").replaceAll("'", "\\'") + "', '" + sourcefile + "', '" + responseType + "', '" + messageType + "');");
	}
};

var TYPES = {
    'undefined': 'undefined',
    'number': 'number',
    'boolean': 'boolean',
    'string': 'string',
    '[object Function]': 'function',
    'function': 'function',
    '[object RegExp]': 'regexp',
    '[object Array]': 'array',
    '[object Date]': 'date',
    '[object Error]': 'error',
    'array': 'array',
    'object': 'object',
},
TOSTRING = Object.prototype.toString;

function toType(o) {

    if (o === null && typeof o == "object")
        return "object";

    if (typeof o == "function")
        return "function";

    if (typeof o != "undefined" && typeof o.toString == "undefined")
        o.toString = function () {  };

    return TYPES[TOSTRING.call(o)] || TYPES[typeof o] || (o ? 'object' : 'null');
}

Object.defineProperty(window, '__stack', {
    get: function () {
        var orig = Error.prepareStackTrace;
        Error.prepareStackTrace = function (_, stack) {
            return stack;
        };
        var err = new Error;
        Error.captureStackTrace(err, arguments.callee);
        var stack = err.stack;
        Error.prepareStackTrace = orig;
        return stack;
    }
});

Object.defineProperty(window, '__function', {
    get: function () {
        var func = __stack[__stack.length - 2].getFunctionName();

        if (func == "console.log")
            func = __stack[__stack.length - 1].getFunctionName();

        return func;
    }
});

Object.defineProperty(window, '__length', {
    get: function () {
        return __stack.length;
    }
});

String.prototype.replaceAll = function (find, replace) {
    var str = this;
    return str.replace(new RegExp(find.replace(/[-\/\\^$*+?.()|[\]{}]/g, '\\$&'), 'g'), replace);
};

console.log = function (message) {

        var serialized = serialize(message);

        var line = __line;
        var func = __function;
        var source = App.getFormFilePath(App.getCurrentFormName());
        var responseType = "&laquo;";

        if (func === "" || (line == 3 && func == "Object.defineProperty.get")) {
            line = 0;
            func = "DBG";
            source = "VM029";
            responseType = "&raquo;";
        }
        
        var type = toType(message);

        if (type == "string" && message == "undefined")
            type = "undefined";

        var escaped_message_final = serialized.replaceAll("\r\n", "<br>").replaceAll("\n", "<br>").replaceAll("\\", "\\\\").replaceAll("\"", "\\\"");

        App.invokeOnFormAsync("debug", 'addConsoleMessageToList("' + App.getCurrentFormName() + '", "ConsoleMessage", "' + line + ':' + func + '", "' + /* TODO: serialize message if it is not a string */ escaped_message_final + '", "' + source + '", "' + responseType + '", "' + toType(message) + '");');

    
};

//TODO:  Implement these functions

/*

console.warn = function () {

}

console.error = function () {

}

console.debug = function () {

}

console.info = function () {

}*/

var DWARF_OBJECTS_HIDDEN = {};

String.prototype.escapeHTMLEntities = function () {
    var tagsToReplace = {
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;'
    };
    return this.replace(/[&<>]/g, function (tag) {
        return tagsToReplace[tag] || tag;
    });
};

var objectType = function (obj) {
    return ({}).toString.call(obj).match(/\s([a-zA-Z]+)/)[1];
};


function serialize(object, forcetype, showhide, recursion)
{
     var type = toType(object);

     if (typeof forcetype !== "undefined")
         type = forcetype;

     if (typeof recursion == "undefined")
         recursion = 0;

     if (typeof showhide == "undefined")
         showhide = false;

     var final = "undefined";

     var recur_t = "";

     for (var cnt = 0; cnt < recursion; cnt++)
         recur_t += "&nbsp;&nbsp;";

    switch (type)
    {
        case 'string':

            if (object == "DWARF.CUSTOM_TYPES.UNDF_HELPR_STR")
                final = "undefined";
            else {

                if (showhide && object.length > 40) {

                    var ellips_chars = object.slice(0, 40);

                    final = "<span class=\"function-hidden\"><span class=\"showhide\" onclick=\"var elm = this.parentNode.children[1]; if (elm.style.display == 'none') { elm.style.display = ''; this.innerHTML = '▼ ';  } else { elm.style.display = 'none'; this.innerHTML = '► &quot;<span class=\\'string-enc\\'>" + ellips_chars.escapeHTMLEntities().replaceAll(" ", "&nbsp;") + "</span>&quot;&hellip;';  }\">&#9658; &quot;<span class=\"string-enc\">" + ellips_chars.escapeHTMLEntities().replaceAll(" ", "&nbsp;") + "</span>&quot;&hellip;</span><span style=\"display:none;\">&quot;<span class=\"string-enc\">" + object.escapeHTMLEntities().replaceAll(" ", "&nbsp;") + "</span>&quot;</span></span>";
                }
                else
                    final = "&quot;<span class=\"string-enc\">" + object.escapeHTMLEntities().replaceAll(" ", "&nbsp;") + "</span>&quot;";

                final = final;
            }

            break;

        case 'undefined':
            final = "undefined";
            break;

        case 'number':
            final = "<span class=\"number-enc\">" + object + "</span>";
            break;

        case 'null':
            final = "null";
            break;

        case 'function':
            if (showhide)
                final = "<span class=\"function-hidden\"><span class=\"showhide\" onclick=\"var elm = this.parentNode.children[1]; if (elm.style.display == 'none') { elm.style.display = ''; this.innerHTML = '▼ ';  } else { elm.style.display = 'none'; this.innerHTML = '► <i>function</i>';  }\">&#9658; <i>function</i></span><span style=\"display:none;\">" + object.toString().trim().escapeHTMLEntities().replaceAll(" ", "&nbsp;") + "</span></span>";
            else
                final = (object.toString().trim().escapeHTMLEntities().replaceAll(" ", "&nbsp;"));

            break;

        case 'array':

            final = "<i>array</i> [";

            for (var i = 0; i < object.length; i++)
            {
                final += serialize(object[i], undefined, true, recursion+1) + ", ";
            }

            if (object.length > 0)
                final = final.slice(0, final.length - 2);

            final += "]";

            break;

        case 'object':

            if (showhide) {

                var id = Math.random().toString();

                DWARF_OBJECTS_HIDDEN[id] = object;

                final = "<span class=\"function-hidden\"><span class=\"showhide\" onclick=\"var elm = this.parentNode.children[1]; if (elm.style.display == 'none') { elm.style.display = ''; this.innerHTML = '▼ '; elm.innerHTML = '&hellip;'; App.invokeOnFormAsync('" + App.getCurrentFormName() + "', 'var DWARF_HIDDEN_OBJ_SERIALIZED = serialize(DWARF_OBJECTS_HIDDEN[\\'\' + elm.id + \'\\'], undefined, undefined, " + (recursion + 1) + "); setHiddenObjectSer(\\'\' + elm.id + \'\\', DWARF_HIDDEN_OBJ_SERIALIZED); ');  } else { elm.style.display = 'none'; this.innerHTML = '► object <i>" + objectType(object) + "</i>'; elm.innerHTML = '';  }\">&#9658; object <i>" + objectType(object) + "</i></span><span id=\"" + id + "\" style=\"display:none;\"></span></span>";
            }
            else
            {
                try {
                    Object.keys(object);
                }
                catch (ex) {
                    return "object <i>Object</i> undefined";
                }

                if (typeof object == "function")
                    return serialize(object, "function", undefined, recursion+1);

                final = "object <i>" + objectType(object) + " {</i><br>";

                if (recursion === 0) {
                    recur_t += "&nbsp;&nbsp;";
                    recursion++;
                }

                for (var prop in object)
                {
                    final += recur_t + serialize(prop, "objectkey", undefined, recursion) + ": " + serialize(object[prop], undefined, true, recursion) + ",<br>";
                }

                if (final.length > 19) {
                    final = final.slice(0, final.length - 5) + "<br>";
                }

                final += recur_t.slice(0, recur_t.length - 12) + "<i>}</i>";
            }

            break;

        case 'objectkey':

            final = "<span class=\"objectkey\">" + object + "</span>";

            break;

        case 'boolean':

            final = "<span class=\"boolean\">" + object.toString() + "</span>";

            break;

        default:
            final = "_________UNIMPLEMENTED_DATA_TYPE_________:  " + type;
            break;
     }

    return final;
}

function setHiddenObjectSer(id, serialized)
{
    var code = 'updateHiddenObjectInnerHtml(\'' + id + '\', \'' + DWARF_HIDDEN_OBJ_SERIALIZED.replaceAll('\\\\\\\\\\\\\\\\\'', '\\\\\\\\\\\\\\\\\\\\\'').replaceAll('\\\\\\\\\\\\\\\'', '\\\\\\\\\\\\\\\\\\\'').replaceAll('\\\\\\\\\\\\\\\'', '\\\\\\\\\\\\\\\\\'').replaceAll('\\\\\\\\\\\\\'', '\\\\\\\\\\\\\\\'').replaceAll('\\\\\\\\\\\'', '\\\\\\\\\\\\\'').replaceAll('\\\\\\\\\'', '\\\\\\\\\\\'').replaceAll('\\\\\\\'', '\\\\\\\\\'').replaceAll('\\\\\'', '\\\\\\\'').replaceAll('\\\'', '\\\\\'').replaceAll('\'', '\\\'').replaceAll("\r\n", "<br>").replaceAll("\n", "<br>") + '\');';
    App.invokeOnFormAsync('debug', code);
}