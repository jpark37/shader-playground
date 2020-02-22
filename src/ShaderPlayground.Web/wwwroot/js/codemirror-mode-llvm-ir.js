// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

(function (mod) {
    if (typeof exports == "object" && typeof module == "object") // CommonJS
        mod(require("../../lib/codemirror"));
    else if (typeof define == "function" && define.amd) // AMD
        define(["../../lib/codemirror"], mod);
    else // Plain browser env
        mod(CodeMirror);
})(function (CodeMirror) {
    "use strict";

    CodeMirror.defineMode("llvm-ir", function (config, parserConfig) {
        function tokenString(quote) {
            return function (stream) {
                var escaped = false, next, end = false;
                while ((next = stream.next()) !== null) {
                    if (next == quote && !escaped) {
                        end = true;
                        break;
                    }
                    escaped = !escaped && next == "\\";
                }
                return "string";
            };
        }

        function contains(words, word) {
            if (typeof words === "function") {
                return words(word);
            } else {
                return words.propertyIsEnumerable(word);
            }
        }

        var keywords = "true,false,declare,define,global,constant," +
            "private,internal,available_externally,linkonce,linkonce_odr,weak,weak_odr,appending,dllimport,dllexport," +
            "common,default,hidden,protected,unnamed_addr,externally_initialized,extern_weak,external,thread_local,localdynamic,initialexec,localexec,zeroinitializer,undef," +
            "null,to,tail,musttail,target,triple,unwind,deplibs,datalayout,volatile,atomic,unordered,monotonic,acquire,release,acq_rel,seq_cst,singlethread," +
            "nnan,ninf,nsz,arcp,fast,nuw,nsw,exact,inbounds,align,addrspace,section,alias,module,asm,sideeffect,alignstack,inteldialect,gc,prefix,prologue," +
            "null,to,tail,musttail,target,triple,unwind,deplibs,datalayout,volatile,atomic,unordered,monotonic,acquire,release,acq_rel,seq_cst,singlethread," +
            "ccc,fastcc,coldcc,x86_stdcallcc,x86_fastcallcc,x86_thiscallcc,x86_vectorcallcc,arm_apcscc,arm_aapcscc,arm_aapcs_vfpcc,msp430_intrcc," +
            "ptx_kernel,ptx_device,spir_kernel,spir_func,intel_ocl_bicc,x86_64_sysvcc,x86_64_win64cc,webkit_jscc,anyregcc," +
            "preserve_mostcc,preserve_allcc,ghccc,cc,c," +
            "attributes," +
            "alwaysinline,argmemonly,builtin,byval,inalloca,cold,convergent,dereferenceable,dereferenceable_or_null,inlinehint,inreg,jumptable," +
            "minsize,naked,nest,noalias,nobuiltin,nocapture,noduplicate,noimplicitfloat,noinline,nonlazybind,nonnull,noredzone,noreturn," +
            "nounwind,optnone,optsize,readnone,readonly,returned,returns_twice,signext,sret,ssp,sspreq,sspstrong,safestack," +
            "sanitize_address,sanitize_thread,sanitize_memory,uwtable,zeroext," +
            "type,opaque," +
            "comdat," +
            "any,exactmatch,largest,noduplicates,samesize," + // Comdat types
            "eq,ne,slt,sgt,sle,sge,ult,ugt,ule,uge,oeq,one,olt,ogt,ole,oge,ord,uno,ueq,une," +
            "xchg,nand,max,min,umax,umin," +
            "x,blockaddress," +
            "distinct," + // Metadata types.
            "uselistorder,uselistorder_bb," + // Use-list order directives.
            "personality,cleanup,catch,filter".split(',');

        var typeKeywords = "void,half,float,double,x86_fp80,fp128,ppc_fp128,label,metadata,x86_mmx".split(',');

        var numberStart = /[\d\.]/;
        var number = /^(?:0x[a-f\d]+|0b[01]+|(?:\d+\.?\d*|\.\d+)(?:e[-+]?\d+)?)(u|ll?|l|f)?/i;

        function isAlnum(ch) {
            return ('a' <= ch && ch <= 'z') || ('A' <= ch && ch <= 'Z') || ('0' <= ch && ch <= '9');
        }

        function isMetadataChar(ch) {
            // digits can't be leads, but already identified by leading '$'
            return isAlnum(ch) || ch == '-' || ch == '$' || ch == '.' || ch == '_' || ch == '\\';
        }

        function isInteger(str) {
            var n = Math.floor(Number(str));
            return String(n) === str && n >= 0;
        }

        var isPunctuationChar = /[\[\]{}\(\),;\:\.]/;

        // Interface

        return {
            token: function (stream) {
                if (stream.eatSpace()) return null;
                var ch = stream.next();
                if (ch == ';') {
                    stream.skipToEnd();
                    return "comment";
                }
                if (ch == '"' || ch == "'") {
                    return tokenString(ch)(stream);
                }
                if (ch == '@') {
                    stream.eatWhile(/[\w\$\.]/);
                    return "def";
                }
                if (ch == '%') {
                    stream.eatWhile(/[\w\$\.]/);
                    return "variable-2";
                }
                if (isPunctuationChar.test(ch)) {
                    return null;
                }
                if (numberStart.test(ch)) {
                    stream.backUp(1)
                    if (stream.match(number)) return "number"
                    stream.next()
                }
                if (ch == '!') {
                    stream.eatWhile(isMetadataChar);
                    return "attribute";
                }

                stream.eatWhile(/[\w\$_\xa1-\uffff]/);
                var cur = stream.current();
                if (keywords.indexOf(cur) !== -1) {
                    return "keyword";
                }
                if (typeKeywords.indexOf(cur) !== -1) {
                    return "variable-3";
                }
                if (cur.charAt(0) == 'i' && isInteger(cur.substring(1))) {
                    return "variable-3";
                }
                return "word";
            }
        };
    });

    CodeMirror.defineMIME("text/x-llvm-ir", "llvm-ir");
});