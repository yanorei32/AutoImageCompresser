CSC			= /cygdrive/c/windows/microsoft.net/framework/v4.0.30319/csc.exe

REPO		= https://github.com/Yanorei32/AutoImageCompresser
PROJ_NAME	= AutoImageCompresser
RELEASE_DIR	= $(PROJ_NAME)

SRCS		= src\\main.cs

TARGET		= $(PROJ_NAME)\(CreateShortcut\).exe

CSC_FLAGS	= /nologo \
			  /utf8output

.PHONY: all
all: $(RELEASE_DIR)/$(TARGET) \
	$(RELEASE_DIR)/LICENSE.txt \
	$(RELEASE_DIR)/README.url \
	$(RELEASE_DIR)/configure.ini

$(RELEASE_DIR)/$(TARGET): $(SRCS)
	-mkdir -p $(RELEASE_DIR)
	$(CSC) $(CSC_FLAGS) /out:$(RELEASE_DIR)\\$(TARGET) $(SRCS)

$(RELEASE_DIR)/configure.ini: default.configure.ini
	-mkdir -p $(RELEASE_DIR)
	cp \
		default.configure.ini \
		$(RELEASE_DIR)/configure.ini

$(RELEASE_DIR)/LICENSE.txt: LICENSE
	-mkdir -p $(RELEASE_DIR)
	cp \
		LICENSE \
		$(RELEASE_DIR)/LICENSE.txt

$(RELEASE_DIR)/README.url:
	-mkdir -p $(RELEASE_DIR)
	echo -ne \
		"[InternetShortcut]\r\nURL=$(REPO)/blob/master/README.md" \
		> "$(RELEASE_DIR)/README.url"

.PHONY: genzip
genzip: $(PROJ_NAME).zip

$(PROJ_NAME).zip: all
	rm -f $(PROJ_NAME)/*.lnk
	zip -r $(PROJ_NAME).zip $(PROJ_NAME)

.PHONY: clean
clean:
	rm -r $(PROJ_NAME)


