CSC			= /cygdrive/c/windows/microsoft.net/framework/v4.0.30319/csc.exe
TARGET		= AutoImageCompresser.exe
SRC			=	src\\main.cs

ZIP_DEPS	= 

CSC_FLAGS		=	/nologo \
					/utf8output \
					# /win32icon:res\\icon.ico \
					# /resource:res\\icon.ico,icon \
					# /resource:res\\logo.png,logo

all: $(TARGET)
$(TARGET): $(SRC) $(DEPS)
	$(CSC) $(CSC_FLAGS) /out:$(TARGET) $(SRC)

.PHONY: clean
clean:
	rm $(TARGET)


