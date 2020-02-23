CSC			= /cygdrive/c/windows/microsoft.net/framework/v4.0.30319/csc.exe

PROJ_NAME	= AutoImageCompresser
TARGET		= $(PROJ_NAME).exe
SRC			= src\\main.cs

ZIP_DEPS	= LICENSE README.md

CSC_FLAGS		=	/nologo \
					/utf8output \
					# /win32icon:res\\icon.ico \
					# /resource:res\\icon.ico,icon \
					# /resource:res\\logo.png,logo

all: $(PROJ_NAME)/$(TARGET)
$(PROJ_NAME)/$(TARGET): $(SRC)
	$(CSC) $(CSC_FLAGS) /out:$(PROJ_NAME)/$(TARGET) $(SRC)

.PHONY: genzip
genzip: $(PROJ_NAME).zip

$(PROJ_NAME).zip: all
	rm -f $(PROJ_NAME)/*.lnk
	zip -r $(PROJ_NAME).zip $(PROJ_NAME)

.PHONY: clean
clean:
	rm $(PROJ_NAME)/$(TARGET)


