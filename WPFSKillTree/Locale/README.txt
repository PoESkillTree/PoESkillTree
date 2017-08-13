
Table of contents

1. Introduction
  1.1. Language identifiers
  1.2. Translation catalogs
  1.3. How it works
  1.4. Message file format (PO file)
  1.5. Message file header
  1.6. C# format strings
  1.7. Fuzzy entries
  1.8. Plural forms
  1.9. Contexts
2. Translators
  2.1. Create translation catalog
  2.2. Copy files
  2.3. Initialize message file
    2.3.1. Use "msginit.exe" tool from command-line (requires knowledge of PO file format)
    2.3.2. Use PO editing tool (requires use of third-party tool)
  2.4. Translate
  2.5. How to see updated translation
3. Developers
  3.1. How to write localized C# source
  3.2. How to write localized XAML source
  3.3. Other resource localization
  3.4. Translation catalog creation
  3.5. Actualizing translation catalogs
4. Appendix
  Table 1: All supported languages


1. Introduction

This document should be read by both translators and developers. It descibes all basics and concepts used
by localization implementation with references to optional more detailed information about specific topics.

The core of localization in PoESkillTree application is simple message translation based on GNU gettext
(see http://www.gnu.org/software/gettext/manual/html_node/), which provides features and tools to allow
precise localization of the application.

Beside message translation, the localization also provides translation of other resources (e.g. help document).

If you are translator and you are not much into learning, the use of PO editing tool is highly suggested.
Though, you should still read about important topics like C# format strings, fuzzy entries, plural forms and contexts,
which you will run into even when using PO editing tool.


1.1. Language identifiers

The languages in .NET and also in PoESkillTree are identified by culture name, or sometimes referred as language tag.
This culture name is composed of language code (lowercase two-letter code derived from ISO 639-1) and country code
(uppercase two-letter code derived fro ISO 3166) separated by minus ("-") character.

The country code is necessary to distinguish the language dialect used in different regions. For example, "pt-BR"
identifies Portuguese language spoken in Brazil, which can differ from Portuguese spoken in Portugal (identified by
"pt-PT").

Some languages are digraphic, i.e. they can use two or more writing systems (alphabets). To distinguish between writing
system used in translation, there is third subtag of culture name and it's called variant. An example of such
digraphic language is Serbian language which can use both Latin and Cyrillic writing system. The exact culture name for
Serbian language written in Cyrillic and used in Serbia would be "sr-Cyrl-RS".

The message files containing translated messages uses PO file format defined by GNU gettext which was originally
developed for *NIX platforms, and as such, they use *NIX locale identifier to identify the language of translation.
The *NIX locale identifier is similar to .NET culture name, but uses different order of code parts and different
separators. For example, "pt-BR" culture name corresponds to "pt_BR" locale identifier and "sr-Cyrl-RS" becomes
"sr_RS@cyrl". So, while .NET uses "language-variant-region" format, *NIX locale uses "language_region@variant".


1.2. Translation catalogs

The translation catalogs in PoESkillTree are represented by folders inside of "Locale" directory and are named
using culture name according to language they provide translation into.

The translation catalog folder contains multiple files, but most important one is message file "Messages.po".
This file contains all translated messages used in application.

Other files can be documents which are displayed within application. For example, Help.md file is document displayed by
"View Help" menu command and it uses simple Markdown syntax to format its content (see
https://guides.github.com/features/mastering-markdown/ for more information about Markdown syntax).

It should be noted, that all text files (message file and rest of text documents) must use UTF-8 encoding.


1.3. How it works

The whole concept of message translation is based on fact, that all messages in application are written in single
language, which is English (United States). As such, they don't need to be translated while application is running
in English language.

Once the application is set to use different language than English, the translation occurs and messages are being
looked up in message file of translation catalog for selected language. If message is not found in message file
or it's not translated (translated message is empty string), then default English message will be displayed instead.
This ensures that for whatever reason message cannot be translated it will still be displayed.

Similar mechanism is used to find translated document, in which case if the document is not found in translation
catalog folder, then default English document will be displayed.


1.4. Message file format (PO file)

The format of this message file is plain text with a simple structure. Actual knowledge of its format is not required,
if PO editing tool is used.

A PO file is made up of many entries, each entry holding the relation between an original untranslated string
in English and its corresponding translation. One PO file entry has the following schematic structure:

    # comments containing:
    #: source references
    #, flags
    #~ obsolete translations
    msgid "untranslated-string"
    msgstr "translated-string"

These entries are separated by blank line. The comment lines starting with "#" character don't play any role
in translation, but they can be useful to translators, developers and PO editing tools, and they should be preserved.

The comments starting with "#:" hold references to every line of source code where particular string is being used.
This helps to understand the context of message being translated, if it's not obvious from untranslated English string.

The comments starting with "#," hold flags specifying special properties of message. During translation you will
encounter only "csharp-format" and "fuzzy" flags. The "csharp-format" flag specifies that untranslated string contains
C# format items, and as such, the translated string will be used as format string and should contain all format items
of original untranslated string. The "fuzzy" flag specifies that there is chance that translation may not be correct
any longer and translation should be revised. More details about C# format strings and fuzzy entries are explained
later.

The comments starting with "#~" are used to "remove" messages which are no longer used in application. For example
piece of source code using particular message was removed and there is no need for this message anymore. These comments
can be safely removed. But if they are kept and previously removed source code using this message will be brought back,
then this entry will be automatically transformed to regular translated message by development process.

The most important lines of PO entry are those starting with keywords (e.g. msgid, msgstr, etc.).

The "msgid" keyword identifies original untranslated string in English. This string should not be changed as it serves
as identification of particular message during translation in application.

The "msgstr" keyword identifies actual translated string. The content of this string is changed by translator to
provide translation of untranslated string.

It should be noted that both untranslated and translated string are represented as C# strings, and as such, they can
contain control characters like new lines ("\n") or tabs ("\t"), which should be preserved. Also it's important to know
that string can be split into multiple lines without affecting actual content of string. For example string:

    msgid "line 1"
    "line 2"
    "line 3"

is identical to string:

    msgid "line 1line 2line 3"

The "msgid_plural" keyword identifies general plural form of untranslated string in English. Once this line is present,
it denotes an entry with plural forms, that is, a message where the text depends on a cardinal number. The general form
of the message, in English, is the "msgid_plural" line. The "msgid" line is the English singular form, that is,
the form for when the number is equal to 1. More details about plural forms are explained later.

The "msgctxt" keyword defines a context for translation. The context serves to disambiguate messages with the same
untranslated-string. It is possible to have several entries with the same untranslated-string in a PO file, provided
that they each have a different context. More details about contexts are explained later.

For more details about PO file format see http://www.gnu.org/software/gettext/manual/html_node/PO-Files.html#PO-Files.


1.5. Message file header

The message file header is the first entry of message file with "msgid" of empty string. It's not used in translation,
but it contains important information about language of message file, beside other information mostly used by PO
editing tools.

More details about message file header can be found at
http://www.gnu.org/software/gettext/manual/html_node/Header-Entry.html#Header-Entry.


1.6. C# format strings

The PoESkillTree is written in C# language, and as such, it uses its string formatting capabilities. The formatting
in C# is simply based on replacing placeholders (format items) in format string with actual values.

Example of C# formatting:

    string formatted = string.Format("Numbers {0} and {1} are equal!", number1, number2);

In this example, the format item "{0}" will be replaced with value of number1 variable and "{1}" with value of number2
variable.

The message entry in Slovak message file for this example would look like this:

    #, csharp-format
    msgid "Numbers {0} and {1} are equal!"
    msgstr "Čísla {0} a {1} su zhodné!"

These format items are positional, that means that "{0}" will be always replaced with first argument and "{1}" with
second. As such, it's safe to change order of format items in translated string if translation requires it.

For more details about C# formatting see
https://msdn.microsoft.com/en-us/library/system.string.format%28v=vs.110%29.aspx#Starting.


1.7. Fuzzy entries

The message file entries marked with "fuzzy" flag are subject to revision by translator. They are generated
automatically during development process, if change in original untranslated string is detected.

The reason for change can be as simple as correction of typo, or it can be more serious change which can lead
to different translation.

During the translation process in application, the fuzzy entries are treated as regular entries, so they still provide
translation.

Once the fuzzy entries are checked by translator or corrected where required, the fuzzy flag can be removed.
If you are editing PO file manually, the removing of the fuzzy flag means just deleting "#, fuzzy" comment from entry,
unless there is also another flag (e.g. csharp-format), then only "fuzzy" keyword must be deleted while keeping
the other keyword(s).


1.8. Plural forms

It's common for a language to have more than one plural form as opposite to English, which have only one plural form.
This means that depending on number of plural forms, multiple translated strings can exist for one particular message.
Because there are multiple possible translations, the numeric value is used to determine which plural form will
be used.

This is an example of Slovak PO file entry for message with plural forms:

    #, csharp-format
    msgid "{0} file deleted"
    msgid_plural "{0} files deleted"
    msgstr[0] "{0} súbor bol zmazaný"
    msgstr[1] "{0} súbory boli zmazané"
    msgstr[2] "{0} súborov bolo zmazaných"

The difference from regular message without plural forms is, that untranslated string denoted by "msgid" is followed
by "msgid_plural" untranslated string. The "msgid" untranslated string is singular form of message in English, while
"msgid_plural" is plural form of same message in English.

The last difference is, that instead of one "msgstr" translated string, there are three translated strings. That's
because Slovak language has two plural forms beside singular form. One is used with values of 2, 3 and 4, and second
form is used with value of 0 and all values above 5 including.

As you can see, the translated strings are distinguished by "[0]", "[1]" and "[2]" indices. First translated string
with index of "[0]" is singular form, where two other translated strings with indices of "[1]" and "[2]" are plural
forms of same message.

So, how are correct translated strings picked up by localization? The number of plural forms and formula used
to determine the index of translated string of correct plural form are defined in header of the message file.

This is line from Slovak message file header:

    "Plural-Forms: nplurals=3; plural=(n==1) ? 0 : (n>=2 && n<=4) ? 1 : 2;\n"

The expression "nplurals=3" means that there is total of 3 plural forms (it's 3, because singular form is technically
considered as case of plural form). The expression "plural=(n==1) ? 0 : (n>=2 && n<=4) ? 1 : 2" is formula written
using C/C# programming language syntax. The "n" in this formula is the numeric value, using which can be formula
evaluated and its resulting value used as index to determine correct plural form.

As you can see, the formula evaluates to 0 for numeric value "n" equal to 1. So, first translated string with index
of "[0]" will be used, which is singular form. If numeric value "n" is between 2 and 4, then formula evaluates
to 1 and second translated string with index of "[1]" will be used, which is first plural form. And last, if numeric
value "n" is not 1 and not between 2 and 4, then formula evaluates to 2, which indicates the second and last plural
form.

More details about plural forms can be found at
http://www.gnu.org/software/gettext/manual/html_node/Plural-forms.html#Plural-forms and
http://www.gnu.org/software/gettext/manual/html_node/Translating-plural-forms.html#Translating-plural-forms.


1.9. Contexts

There are situations where certain words or phrases have different meaning in different contexts, and as such, they
need to be translated differently. To help with this issue, the localization provides way to establish a context
for untranslated strings. This context together with untranslated string, then identifies particular message uniquely.
So, multiple identical untranslated strings can exist in same message file, provided that they each have a different
context.

Here is an example of such message:

    msgid "Help"
    msgstr "Pomocník"

    msgctxt "exclamation"
    msgid "Help"
    msgstr "Pomoc"

In this example, there are two same untranslated strings "Help". First one, doesn't have any context
specified, so it will be most common use within application, which is "Help" as tool to access help document in menu.
The second one, has context "exclamation" specified to indicate that "Help" should be translated in such context.

There is usually no need for contexts, as the translation is done for menu commands, form and button labels,
and informational and error messages. The short, few words long messages, are usually menu commands, and form
and button labels. The informational and error messages are usually full sentences.


2. Translators


2.1. Create translation catalog

Simply create folder in "Locale" folder with culture name you are going to make translation for. The culture name
should include all tags required to identify language and other properties of translation.

The correct culture name is very important to let people know what to expect and not to confuse them with region
specific terms. For example, if you live in Brazil and you are going to make Portuguese translation, you should name it
as "pt-BR", unless you are sure you can translate everything into Portuguese without using region specific terms
or grammatical constructs used only in either Brazil or Portugal.

If language has only single region of use (e.g. "sk-SK"), then there is no need for region to be specified in culture
name.

If language has multiple regions of use and you are able to translate everything without using region specific terms
or grammatical constructs, then you can name the folder with language code only ("pt" in case of Portuguese). Such
translation catalog will be used for all regions where language is spoken.

If language you are going to translate into is digraphic (can use two or more alphabets), then you have to include
also variant identifying alphabet you are going to use in translation. There is possibility that someone else might do
a translation using other alphabet(s). Using Serbian language as example, the correct culture name for translation
catalog can be one of these "sr-Latn", "sr-Cyrl" for region neutral translations and "sr-Latn-RS", "sr-Latn-BA",
"sr-Latn-ME", "sr-Cyrl-RS", "sr-Cyrl-BA", "sr-Cyrl-ME" for region specific translations using either Latin or Cyrillic
alphabet.

For list of all supported languages see 4. Appendix, Table 1. This list contains all culture names (both region neutral
and region specific) you can pick from. Remember, if the language you are going to translate into is spoken in only one
region, you can pick region neutral culture name (only language code), such as "af" for Afrikaans spoken only in South
Africa.


2.2. Copy files

Once the translation catalog folder is created, you can simply copy content of "en-US" folder containing English
resources into new translation catalog folder.


2.3. Initialize message file

There are multiple ways to initialize message file.

The basic one is to copy "Messages.pot" file (messages template file, or POT file) found in "Locale" directory
to translation catalog folder and rename it to "Messages.po". Then change message file header to reflect language
of translation (in *NIX locale format), the number of plural forms and plurals formula. This of course, requires
knowledge of PO file format, so it's suggested to use either "msginit.exe" tool or some other PO editing tool.


2.3.1. Use "msginit.exe" tool from command-line (requires knowledge of PO file format)

Open command-line (run "cmd.exe") and change current directory to "Locale" folder of PoESkillTree application
(using "cd" command).
Then execute "msginit.exe" tool with following arguments (example for Portuguese message file):

    D:\PoESkillTree\Locale> msginit.exe -i Messages.pot -o pt-BR\Messages.po -l pt_BR --no-wrap --no-translator

where you replace "pt-BR" folder with name of folder you've created and "pt_BR" argument with *NIX locale identifier
according to .NET culture name (see last paragraph of 1.1. Language identifiers).

Because, input to msginit.exe tool was template message file "Messages.pot", the created message file contains all
messages untranslated. But you can use message file of existing catalog too, to initialize message file. This is
useful, when you are creating translation for same language but different region than existing translation.

Then "msginit.exe" tool execution could look like (example for Portuguese message file):

    D:\PoESkillTree\Locale> msginit.exe -i pt-PT\Messages.po -o pt-BR\Messages.po -l pt_BR --no-wrap --no-translator

which would create message file containing all translations from "pt-PT" message file and it would reduce translation
process to changing only region specific terms or grammatical constructs.

It should be noted, that "msginit.exe" tool doesn't contain complete database of plural forms for each language
(specifically Asian languages). So it's possible that after message file creation, the "Plural-Forms" header field
correction can be required.


2.3.2. Use PO editing tool (requires use of third-party tool)

There are many PO file editing tools online or as downloadable Windows/Mac/*NIX applications.
I'll use Poedit Windows application (http://poedit.net) as example, but concept is pretty much same for other tools.

In menu "File" choose "New from POT/PO file..." and select to either "Messages.pot" template message file located
in "Locale" folder or translated message file of existing catalog, to create either untranslated message file or already
translated message file based on selected message file.

In next dialog choose language you're going to translate into. Be aware that it contains also region neutral languages
so the selected language should match the region and/or alphabet of culture name.

After that you can start translating the messages.

Remember to save file as "Messages.po" into translation catalog folder, as some tools (including Poedit) offer
the file name same as *NIX locale of created message file.


2.4. Translate

The translation can be done using either common text editor able to handle UTF-8 encoding, if you know the PO file
format, or using PO editing tool such as Poedit.

Tools usually offer highlighting of untranslated and fuzzy messages on which translator should focus, together
with simple interface for translation of plural forms (Poedit displays also numeric value examples for each form).

The process of actual translation is very simple. It consist of following steps:

  - Replace each empty translated string ("" after "msgstr" keyword, or its plural form) with correct translation.
    If message has plural forms (is has one or more translated strings indexed by "[number]"), watch for correct index.
    Remember that  first translated string ("msgstr[0]") is always singular form of message with plural forms. For rest
    of plural forms follow the plurals formula in message file header.

    In Poedit plural forms are separated into tabs for each plural form. Just fill in all tabs with translation for
    corresponding plural form.

  - Handle fuzzy entries. This means, you should check if existing translation of such entry is still correct and fix
    the translation if it's required. After that, remove fuzzy flag.

    In Poedit use "Fuzzy" button to remove fuzzy flag from entry.

What should you also look for during translation:

  - The C# format strings (containing {0}, or more complex format items in braces "{..}"), these need to be preserved
    in translated strings. The order of format items in translated string can be changed if translation requires it.

    Poedit checks the presence of format items in translated strings.

  - Menu command accelerator keys (characters used with Alt key to access menu with keyboard), which are denoted
    by underscore character ("_") in front of alphanumeric character. Feel free to change these as you see them fit,
    just keep in mind, that each menu item should use different key to access specific item of menu or submenu.

  - Specific contexts ("msgctxt" string in front of "msgid" untranslated string). These should be taken into account
    when translating the message as they specifies context in which message is used.

    In Poedit context is displayed in brackets "[..]" next to untranslated string.

If you are not certain about some translation of menu command or button label, try to use some other localized
application as reference. It's better to use commonly used terms instead of inventing new ones which would just
confuse users.

If you find that same message, but used in different places, requires to be translated differently due to different
meaning, don't be afraid to file an issue on GitHub asking for context specification for particular message use.
We also want to have natural translation, instead of incorrect or machine like translation.


2.5. How to see updated translation

To see changes to message file in PoESkillTree application, you have to restart the application.


3. Developers

There is not much to do as developer. Just write everything in English (US).


3.1. How to write localized C# source

Use L10n.Message and L10n.Plural static methods to translate messages.

L10n.Message has 1 required argument and one optional:

  - message: The untranslated string in English.
  - context: The optional string specifying context of message.

L10n.Plural has 3 required arguments and one optional:

  - message: The untranslated string in English (singular form).
  - plural: The untranslated string of plural form in English.
  - n: The numeric value based on which singular or plural form will be selected. In English, if value of "n" will
       be 1, singular form will be used. If value of "n" will be anything except 1, then plural form will be used.
  - context: The optional string specifying context of message.

If you need to use string formatting, format translated message instead of untranslated English message. This means
that format string itself must be translated and not formatted result.

Example of simple localized message:

    MessageBox.Show(this, L10n.Message("You have the latest version!"), L10n.Message("No update"));

Example of simple localized message with formatting:

    var message = String.Format(L10n.Message("Do you want to install version {0}?"), release.Version);

Example of localized message with plural forms:

    lblBestResult.Content = string.Format(L10n.Plural("Best result so far: {0} additional point spent",
                                                      "Best result so far: {0} additional points spent",
                                                      (uint)bestSoFar.Count),
                                          bestSoFar.Count);

Don't assign untranslated English string to variable and then use this variable as argument of L10n method. This won't
be detected by gettext tools. So, always use localized strings directly as arguments of L10n method.

If untranslated strings is too long, you can use "+" operator to concatenate multiple shorter strings into one string.

Try to use full sentences if possible, instead of translating parts of sentence and then concatenating the results. This
will lead to less confusion for translators as context of whole sentence will be more obvious.


3.2. How to write localized XAML source

Use "Catalog" custom tag of "clr-namespace:POESKillTree.Localization.XAML" XML namespace to translate messages.
The custom tag has following attributes:

    - Message: The untranslated string in English.
    - Plural: The untranslated string of plural form in English.
    - N: The numeric value based on which singular or plural form will be selected.
    - Context: The optional string specifying context of message.

For messages without plural form, only Message attribute with optional Context attribute is used.
For messages with plural forms also Plural and N attributes have to be set.

Remember to declare XML namespace in root element of XAML file with "xmlns:l" attribute value set to
"clr-namespace:POESKillTree.Localization.XAML".

Example of simple localization:

    <controls:MetroWindow
        ...
        xmlns:l="clr-namespace:POESKillTree.Localization.XAML"
        ...>
        <controls:MetroWindow.Title>
            <l:Catalog Message="About PoESkillTree"/>
        </controls:MetroWindow.Title>
        ...
        <Button ...><l:Catalog Message="Close"/></Button>
    </controls:MetroWindow>

The XAML custom tag doesn't support binding, so if you need to use string formatting or some more complex processing,
do it in C# source and set final translation to property of named XAML element.


3.3. Other resource localization

For now, it's possible to also use localized standalone documents. Untranslated English documents should be placed into
"Locale\en-US" folder. To get content of such document, L10n.ReadAllText static method can be used.

There is also XAML markup extension in Markdown.Xaml C# namespace which can be used to render Markdown documents.
For reference see HelpWindow.xaml and HelpWindow.xaml.cs on use of localized document together with Markdown renderer.


3.4. Translation catalog creation

In development environment of WPFSkillTree project, there is automatized way of message file initialization.

Once, the translation catalog folder is created manually as described in 2.1. Create translation catalog chapter,
running "build-locale.bat" script will also create message file, if it doesn't exist in translation catalog folder.


3.5. Actualizing translation catalogs

This process is done automatically during creation of release, but can be executed manually using "build-locale.bat"
script found in WPFSkillTree project root.

The process simply collects all localized messages from C# and XAML sources and first actualizes message template file
"Messages.pot", which is then used to actualize all existing translation catalogs.


4. Appendix


Table 1: All supported languages

Culture Name		Language Name (Region)
------------------------------------------------------------------------------------------
af              	Afrikaans
af-ZA           	Afrikaans (South Africa)
am              	Amharic
am-ET           	Amharic (Ethiopia)
ar              	Arabic
ar-AE           	Arabic (U.A.E.)
ar-BH           	Arabic (Bahrain)
ar-DZ           	Arabic (Algeria)
ar-EG           	Arabic (Egypt)
ar-IQ           	Arabic (Iraq)
ar-JO           	Arabic (Jordan)
ar-KW           	Arabic (Kuwait)
ar-LB           	Arabic (Lebanon)
ar-LY           	Arabic (Libya)
ar-MA           	Arabic (Morocco)
ar-OM           	Arabic (Oman)
ar-QA           	Arabic (Qatar)
ar-SA           	Arabic (Saudi Arabia)
ar-SY           	Arabic (Syria)
ar-TN           	Arabic (Tunisia)
ar-YE           	Arabic (Yemen)
arn             	Mapudungun
arn-CL          	Mapudungun (Chile)
as              	Assamese
as-IN           	Assamese (India)
az-Cyrl         	Azeri (Cyrillic)
az-Cyrl-AZ      	Azeri (Cyrillic, Azerbaijan)
az-Latn         	Azeri (Latin)
az-Latn-AZ      	Azeri (Latin, Azerbaijan)
ba              	Bashkir
ba-RU           	Bashkir (Russia)
be              	Belarusian
be-BY           	Belarusian (Belarus)
bg              	Bulgarian
bg-BG           	Bulgarian (Bulgaria)
bn              	Bengali
bn-BD           	Bengali (Bangladesh)
bn-IN           	Bengali (India)
bo              	Tibetan
bo-CN           	Tibetan (PRC)
br              	Breton
br-FR           	Breton (France)
bs-Cyrl         	Bosnian (Cyrillic)
bs-Cyrl-BA      	Bosnian (Cyrillic, Bosnia and Herzegovina)
bs-Latn         	Bosnian (Latin)
bs-Latn-BA      	Bosnian (Latin, Bosnia and Herzegovina)
ca              	Catalan
ca-ES           	Catalan (Catalan)
co              	Corsican
co-FR           	Corsican (France)
cs              	Czech
cs-CZ           	Czech (Czech Republic)
cy              	Welsh
cy-GB           	Welsh (United Kingdom)
da              	Danish
da-DK           	Danish (Denmark)
de              	German
de-AT           	German (Austria)
de-CH           	German (Switzerland)
de-DE           	German (Germany)
de-LI           	German (Liechtenstein)
de-LU           	German (Luxembourg)
dsb             	Lower Sorbian
dsb-DE          	Lower Sorbian (Germany)
dv              	Divehi
dv-MV           	Divehi (Maldives)
el              	Greek
el-GR           	Greek (Greece)
en-029          	English (Caribbean)
en-AU           	English (Australia)
en-BZ           	English (Belize)
en-CA           	English (Canada)
en-GB           	English (United Kingdom)
en-IE           	English (Ireland)
en-IN           	English (India)
en-JM           	English (Jamaica)
en-MY           	English (Malaysia)
en-NZ           	English (New Zealand)
en-PH           	English (Republic of the Philippines)
en-SG           	English (Singapore)
en-TT           	English (Trinidad and Tobago)
en-ZA           	English (South Africa)
en-ZW           	English (Zimbabwe)
es              	Spanish
es-AR           	Spanish (Argentina)
es-BO           	Spanish (Bolivia)
es-CL           	Spanish (Chile)
es-CO           	Spanish (Colombia)
es-CR           	Spanish (Costa Rica)
es-DO           	Spanish (Dominican Republic)
es-EC           	Spanish (Ecuador)
es-ES           	Spanish (Spain, International Sort)
es-GT           	Spanish (Guatemala)
es-HN           	Spanish (Honduras)
es-MX           	Spanish (Mexico)
es-NI           	Spanish (Nicaragua)
es-PA           	Spanish (Panama)
es-PE           	Spanish (Peru)
es-PR           	Spanish (Puerto Rico)
es-PY           	Spanish (Paraguay)
es-SV           	Spanish (El Salvador)
es-US           	Spanish (United States)
es-UY           	Spanish (Uruguay)
es-VE           	Spanish (Bolivarian Republic of Venezuela)
et              	Estonian
et-EE           	Estonian (Estonia)
eu              	Basque
eu-ES           	Basque (Basque)
fa              	Persian
fa-IR           	Persian
fi              	Finnish
fi-FI           	Finnish (Finland)
fil             	Filipino
fil-PH          	Filipino (Philippines)
fo              	Faroese
fo-FO           	Faroese (Faroe Islands)
fr              	French
fr-BE           	French (Belgium)
fr-CA           	French (Canada)
fr-CH           	French (Switzerland)
fr-FR           	French (France)
fr-LU           	French (Luxembourg)
fr-MC           	French (Monaco)
fy              	Frisian
fy-NL           	Frisian (Netherlands)
ga              	Irish
ga-IE           	Irish (Ireland)
gd              	Scottish Gaelic
gd-GB           	Scottish Gaelic (United Kingdom)
gl              	Galician
gl-ES           	Galician (Galician)
gsw             	Alsatian
gsw-FR          	Alsatian (France)
gu              	Gujarati
gu-IN           	Gujarati (India)
ha-Latn         	Hausa (Latin)
ha-Latn-NG      	Hausa (Latin, Nigeria)
he              	Hebrew
he-IL           	Hebrew (Israel)
hi              	Hindi
hi-IN           	Hindi (India)
hr              	Croatian
hr-BA           	Croatian (Latin, Bosnia and Herzegovina)
hr-HR           	Croatian (Croatia)
hsb             	Upper Sorbian
hsb-DE          	Upper Sorbian (Germany)
hu              	Hungarian
hu-HU           	Hungarian (Hungary)
hy              	Armenian
hy-AM           	Armenian (Armenia)
id              	Indonesian
id-ID           	Indonesian (Indonesia)
ig              	Igbo
ig-NG           	Igbo (Nigeria)
ii              	Yi
ii-CN           	Yi (PRC)
is              	Icelandic
is-IS           	Icelandic (Iceland)
it              	Italian
it-CH           	Italian (Switzerland)
it-IT           	Italian (Italy)
iu-Cans         	Inuktitut (Syllabics)
iu-Cans-CA      	Inuktitut (Syllabics, Canada)
iu-Latn         	Inuktitut (Latin)
iu-Latn-CA      	Inuktitut (Latin, Canada)
ja              	Japanese
ja-JP           	Japanese (Japan)
ka              	Georgian
ka-GE           	Georgian (Georgia)
kk              	Kazakh
kk-KZ           	Kazakh (Kazakhstan)
kl              	Greenlandic
kl-GL           	Greenlandic (Greenland)
km              	Khmer
km-KH           	Khmer (Cambodia)
kn              	Kannada
kn-IN           	Kannada (India)
ko              	Korean
ko-KR           	Korean (Korea)
kok             	Konkani
kok-IN          	Konkani (India)
ky              	Kyrgyz
ky-KG           	Kyrgyz (Kyrgyzstan)
lb              	Luxembourgish
lb-LU           	Luxembourgish (Luxembourg)
lo              	Lao
lo-LA           	Lao (Lao P.D.R.)
lt              	Lithuanian
lt-LT           	Lithuanian (Lithuania)
lv              	Latvian
lv-LV           	Latvian (Latvia)
mi              	Maori
mi-NZ           	Maori (New Zealand)
mk              	Macedonian
mk-MK           	Macedonian (Former Yugoslav Republic of Macedonia)
ml              	Malayalam
ml-IN           	Malayalam (India)
mn-Cyrl         	Mongolian (Cyrillic)
mn-Cyrl-MN          Mongolian (Cyrillic, Mongolia)
mn-Mong         	Mongolian (Traditional Mongolian)
mn-Mong-CN      	Mongolian (Traditional Mongolian, PRC)
moh             	Mohawk
moh-CA          	Mohawk (Mohawk)
mr              	Marathi
mr-IN           	Marathi (India)
ms              	Malay
ms-BN           	Malay (Brunei Darussalam)
ms-MY           	Malay (Malaysia)
mt              	Maltese
mt-MT           	Maltese (Malta)
nb              	Norwegian, Bokmal
nb-NO           	Norwegian, Bokmal (Norway)
ne              	Nepali
ne-NP           	Nepali (Nepal)
nl              	Dutch
nl-BE           	Dutch (Belgium)
nl-NL           	Dutch (Netherlands)
nn              	Norwegian, Nynorsk
nn-NO           	Norwegian, Nynorsk (Norway)
no              	Norwegian, Bokmal
nso             	Sesotho sa Leboa
nso-ZA          	Sesotho sa Leboa (South Africa)
oc              	Occitan
oc-FR           	Occitan (France)
or              	Oriya
or-IN           	Oriya (India)
pa              	Punjabi
pa-IN           	Punjabi (India)
pl              	Polish
pl-PL           	Polish (Poland)
prs             	Dari
prs-AF          	Dari (Afghanistan)
ps              	Pashto
ps-AF           	Pashto (Afghanistan)
pt              	Portuguese
pt-BR           	Portuguese (Brazil)
pt-PT           	Portuguese (Portugal)
qut             	K'iche
qut-GT          	K'iche (Guatemala)
quz             	Quechua
quz-BO          	Quechua (Bolivia)
quz-EC          	Quechua (Ecuador)
quz-PE          	Quechua (Peru)
rm              	Romansh
rm-CH           	Romansh (Switzerland)
ro              	Romanian
ro-RO           	Romanian (Romania)
ru              	Russian
ru-RU           	Russian (Russia)
rw              	Kinyarwanda
rw-RW           	Kinyarwanda (Rwanda)
sa              	Sanskrit
sa-IN           	Sanskrit (India)
sah             	Yakut
sah-RU          	Yakut (Russia)
se              	Sami, Northern
se-FI           	Sami, Northern (Finland)
se-NO           	Sami, Northern (Norway)
se-SE           	Sami, Northern (Sweden)
si              	Sinhala
si-LK           	Sinhala (Sri Lanka)
sk              	Slovak
sk-SK           	Slovak (Slovakia)
sl              	Slovenian
sl-SI           	Slovenian (Slovenia)
sma             	Sami, Southern
sma-NO          	Sami, Southern (Norway)
sma-SE          	Sami, Southern (Sweden)
smj             	Sami, Lule
smj-NO          	Sami, Lule (Norway)
smj-SE          	Sami, Lule (Sweden)
smn             	Sami, Inari
smn-FI          	Sami, Inari (Finland)
sms             	Sami, Skolt
sms-FI          	Sami, Skolt (Finland)
sq              	Albanian
sq-AL           	Albanian (Albania)
sr-Cyrl         	Serbian (Cyrillic)
sr-Cyrl-BA      	Serbian (Cyrillic, Bosnia and Herzegovina)
sr-Cyrl-CS      	Serbian (Cyrillic, Serbia and Montenegro (Former))
sr-Cyrl-ME      	Serbian (Cyrillic, Montenegro)
sr-Cyrl-RS      	Serbian (Cyrillic, Serbia)
sr-Latn         	Serbian (Latin)
sr-Latn-BA      	Serbian (Latin, Bosnia and Herzegovina)
sr-Latn-CS      	Serbian (Latin, Serbia and Montenegro (Former))
sr-Latn-ME      	Serbian (Latin, Montenegro)
sr-Latn-RS      	Serbian (Latin, Serbia)
sv              	Swedish
sv-FI           	Swedish (Finland)
sv-SE           	Swedish (Sweden)
sw              	Kiswahili
sw-KE           	Kiswahili (Kenya)
syr             	Syriac
syr-SY          	Syriac (Syria)
ta              	Tamil
ta-IN           	Tamil (India)
te              	Telugu
te-IN           	Telugu (India)
tg-Cyrl         	Tajik (Cyrillic)
tg-Cyrl-TJ      	Tajik (Cyrillic, Tajikistan)
th              	Thai
th-TH           	Thai (Thailand)
tk              	Turkmen
tk-TM           	Turkmen (Turkmenistan)
tn              	Setswana
tn-ZA           	Setswana (South Africa)
tr              	Turkish
tr-TR           	Turkish (Turkey)
tt              	Tatar
tt-RU           	Tatar (Russia)
tzm-Latn        	Tamazight (Latin)
tzm-Latn-DZ     	Tamazight (Latin, Algeria)
ug              	Uyghur
ug-CN           	Uyghur (PRC)
uk              	Ukrainian
uk-UA           	Ukrainian (Ukraine)
ur              	Urdu
ur-PK           	Urdu (Islamic Republic of Pakistan)
uz-Cyrl         	Uzbek (Cyrillic)
uz-Cyrl-UZ      	Uzbek (Cyrillic, Uzbekistan)
uz-Latn         	Uzbek (Latin)
uz-Latn-UZ      	Uzbek (Latin, Uzbekistan)
vi              	Vietnamese
vi-VN           	Vietnamese (Vietnam)
wo              	Wolof
wo-SN           	Wolof (Senegal)
xh              	isiXhosa
xh-ZA           	isiXhosa (South Africa)
yo              	Yoruba
yo-NG           	Yoruba (Nigeria)
zh-CN           	Chinese (Simplified, PRC)
zh-HK           	Chinese (Traditional, Hong Kong S.A.R.)
zh-MO           	Chinese (Traditional, Macao S.A.R.)
zh-SG           	Chinese (Simplified, Singapore)
zh-TW           	Chinese (Traditional, Taiwan)
zu              	isiZulu
zu-ZA           	isiZulu (South Africa)
