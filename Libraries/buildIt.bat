@ECHO OFF
SET IKVMC=C:\Programming\Utilities\ikvm\bin\ikvmc.exe

@ECHO Building marytts5.2.dll
%IKVMC% -classloader:ikvm.runtime.AppDomainAssemblyClassLoader -out:marytts5.2.dll -target:library marytts-runtime-5.2-SNAPSHOT-jar-with-dependencies.jar>NUL 2>&1

@ECHO .
@ECHO .
@ECHO Building marytts-lang-de.dll
%IKVMC% -classloader:ikvm.runtime.AppDomainAssemblyClassLoader -out:marytts-lang-de.dll -target:library marytts-lang-de-5.2-SNAPSHOT.jar -r:marytts5.2.dll>NUL 2>&1
@ECHO Building marytts-lang-en.dll
%IKVMC% -classloader:ikvm.runtime.AppDomainAssemblyClassLoader -out:marytts-lang-en.dll -target:library marytts-lang-en-5.2-SNAPSHOT.jar -r:marytts5.2.dll>NUL 2>&1
@ECHO Building marytts-lang-fr.dll
%IKVMC% -classloader:ikvm.runtime.AppDomainAssemblyClassLoader -out:marytts-lang-fr.dll -target:library marytts-lang-fr-5.2-SNAPSHOT.jar -r:marytts5.2.dll>NUL 2>&1
@ECHO Building marytts-lang-it.dll
%IKVMC% -classloader:ikvm.runtime.AppDomainAssemblyClassLoader -out:marytts-lang-it.dll -target:library marytts-lang-it-5.2-SNAPSHOT.jar -r:marytts5.2.dll>NUL 2>&1
REM @ECHO Building marytts-lang-lb.dll
REM %IKVMC% -classloader:ikvm.runtime.AppDomainAssemblyClassLoader -out:marytts-lang-lb.dll -target:library marytts-lang-lb-5.2-SNAPSHOT.jar -r:marytts5.2.dll -r:marytts-lang-fr.dll>NUL 2>&1
@ECHO Building marytts-lang-ru.dll
%IKVMC% -classloader:ikvm.runtime.AppDomainAssemblyClassLoader -out:marytts-lang-ru.dll -target:library marytts-lang-ru-5.2-SNAPSHOT.jar -r:marytts5.2.dll>NUL 2>&1
@ECHO Building marytts-lang-sv.dll
%IKVMC% -classloader:ikvm.runtime.AppDomainAssemblyClassLoader -out:marytts-lang-sv.dll -target:library marytts-lang-sv-5.2-SNAPSHOT.jar -r:marytts5.2.dll>NUL 2>&1
@ECHO Building marytts-lang-te.dll
%IKVMC% -classloader:ikvm.runtime.AppDomainAssemblyClassLoader -out:marytts-lang-te.dll -target:library marytts-lang-te-5.2-SNAPSHOT.jar -r:marytts5.2.dll>NUL 2>&1
@ECHO Building marytts-lang-tr.dll
%IKVMC% -classloader:ikvm.runtime.AppDomainAssemblyClassLoader -out:marytts-lang-tr.dll -target:library marytts-lang-tr-5.2-SNAPSHOT.jar -r:marytts5.2.dll>NUL 2>&1

@ECHO .
@ECHO .
@ECHO Building voice-bits1-hsmm.dll
%IKVMC% -classloader:ikvm.runtime.AppDomainAssemblyClassLoader -out:voice-bits1-hsmm.dll -target:library voice-bits1-hsmm-5.1.jar -r:marytts5.2.dll -r:marytts-lang-de.dll>NUL 2>&1
@ECHO Building voice-bits3-hsmm.dll
%IKVMC% -classloader:ikvm.runtime.AppDomainAssemblyClassLoader -out:voice-bits3-hsmm.dll -target:library voice-bits3-hsmm-5.1.jar -r:marytts5.2.dll -r:marytts-lang-de.dll>NUL 2>&1
@ECHO Building voice-cmu-bdl-hsmm.dll
%IKVMC% -classloader:ikvm.runtime.AppDomainAssemblyClassLoader -out:voice-cmu-bdl-hsmm.dll -target:library voice-cmu-bdl-hsmm-5.1.jar -r:marytts5.2.dll -r:marytts-lang-en.dll>NUL 2>&1
@ECHO Building voice-cmu-rms-hsmm.dll
%IKVMC% -classloader:ikvm.runtime.AppDomainAssemblyClassLoader -out:voice-cmu-rms-hsmm.dll -target:library voice-cmu-rms-hsmm-5.1.jar -r:marytts5.2.dll -r:marytts-lang-en.dll>NUL 2>&1
@ECHO Building voice-cmu-slt.dll
%IKVMC% -classloader:ikvm.runtime.AppDomainAssemblyClassLoader -out:voice-cmu-slt.dll -target:library voice-cmu-slt-5.1.jar -r:marytts5.2.dll -r:marytts-lang-en.dll>NUL 2>&1
@ECHO Building voice-cmu-slt-hsmm.dll
%IKVMC% -classloader:ikvm.runtime.AppDomainAssemblyClassLoader -out:voice-cmu-slt-hsmm.dll -target:library voice-cmu-slt-hsmm-5.2-SNAPSHOT.jar -r:marytts5.2.dll -r:marytts-lang-en.dll>NUL 2>&1
@ECHO Building voice-dfki-obadiah-hsmm.dl
%IKVMC% -classloader:ikvm.runtime.AppDomainAssemblyClassLoader -out:voice-dfki-obadiah-hsmm.dll -target:library voice-dfki-obadiah-hsmm-5.1.jar -r:marytts5.2.dll -r:marytts-lang-en.dll>NUL 2>&1
@ECHO Building voice-dfki-pavoque-neutral-hsmm.dl
%IKVMC% -classloader:ikvm.runtime.AppDomainAssemblyClassLoader -out:voice-dfki-pavoque-neutral-hsmm.dll -target:library voice-dfki-pavoque-neutral-hsmm-5.1.jar -r:marytts5.2.dll -r:marytts-lang-de.dll>NUL 2>&1
@ECHO Building voice-dfki-poppy.dll
%IKVMC% -classloader:ikvm.runtime.AppDomainAssemblyClassLoader -out:voice-dfki-poppy.dll -target:library voice-dfki-poppy-5.1.jar -r:marytts5.2.dll -r:marytts-lang-en.dll>NUL 2>&1
@ECHO Building voice-dfki-poppy-hsmm.dll
%IKVMC% -classloader:ikvm.runtime.AppDomainAssemblyClassLoader -out:voice-dfki-poppy-hsmm.dll -target:library voice-dfki-poppy-hsmm-5.1.jar -r:marytts5.2.dll -r:marytts-lang-en.dll>NUL 2>&1
@ECHO Building voice-dfki-prudence-hsmm.dll
%IKVMC% -classloader:ikvm.runtime.AppDomainAssemblyClassLoader -out:voice-dfki-prudence-hsmm.dll -target:library voice-dfki-prudence-hsmm-5.1.jar -r:marytts5.2.dll -r:marytts-lang-en.dll>NUL 2>&1
@ECHO Building voice-dfki-spike-hsmm.dll
%IKVMC% -classloader:ikvm.runtime.AppDomainAssemblyClassLoader -out:voice-dfki-spike-hsmm.dll -target:library voice-dfki-spike-hsmm-5.1.jar -r:marytts5.2.dll -r:marytts-lang-en.dll>NUL 2>&1
@ECHO Building voice-enst-camille-hsmm.dl
%IKVMC% -classloader:ikvm.runtime.AppDomainAssemblyClassLoader -out:voice-enst-camille-hsmm.dll -target:library voice-enst-camille-hsmm-5.1.jar -r:marytts5.2.dll -r:marytts-lang-fr.dll>NUL 2>&1
@ECHO Building voice-enst-dennys-hsmm.dll
%IKVMC% -classloader:ikvm.runtime.AppDomainAssemblyClassLoader -out:voice-enst-dennys-hsmm.dll -target:library voice-enst-dennys-hsmm-5.1.jar -r:marytts5.2.dll -r:marytts-lang-fr.dll>NUL 2>&1
@ECHO Building voice-istc-lucia-hsmm.dll
%IKVMC% -classloader:ikvm.runtime.AppDomainAssemblyClassLoader -out:voice-istc-lucia-hsmm.dll -target:library voice-istc-lucia-hsmm-5.1.jar -r:marytts5.2.dll -r:marytts-lang-it.dll>NUL 2>&1
@ECHO Building voice-upmc-jessica.dll
%IKVMC% -classloader:ikvm.runtime.AppDomainAssemblyClassLoader -out:voice-upmc-jessica.dll -target:library voice-upmc-jessica-5.1.jar -r:marytts5.2.dll -r:marytts-lang-fr.dll>NUL 2>&1
@ECHO Building voice-upmc-jessica-hsmm.dll
%IKVMC% -classloader:ikvm.runtime.AppDomainAssemblyClassLoader -out:voice-upmc-jessica-hsmm.dll -target:library voice-upmc-jessica-hsmm-5.1.jar -r:marytts5.2.dll -r:marytts-lang-fr.dll>NUL 2>&1
@ECHO Building voice-upmc-pierre-hsmm.dll
%IKVMC% -classloader:ikvm.runtime.AppDomainAssemblyClassLoader -out:voice-upmc-pierre-hsmm.dll -target:library voice-upmc-pierre-hsmm-5.1.jar -r:marytts5.2.dll -r:marytts-lang-fr.dll>NUL 2>&1

@ECHO .
@ECHO Building finished
