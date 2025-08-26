(function ($) {
    // --- Utility: build URL from template / data-* attrs ---
    function buildUrl(urlTemplate, $input) {
        return urlTemplate.replace(/{(\w+)}/g, function (_, key) {
            const camelKey = key.charAt(0).toLowerCase() + key.slice(1); // clientId, userId, etc.
            const value = $input.data(camelKey);
            if (value == null || value === '') {
                console.warn(`Missing data-${camelKey} for URL: ${urlTemplate}`);
                return '';
            }
            return value;
        });
    }

    // --- Utility: check if all route params exist ---
    function hasAllRouteParams(urlTemplate, $input) {
        let missing = false;
        urlTemplate.replace(/{(\w+)}/g, function (_, key) {
            const camelKey = key.charAt(0).toLowerCase() + key.slice(1);
            const val = $input.data(camelKey);
            if (val == null || val === '') missing = true;
        });
        return !missing;
    }

    // --- Utility: enable/disable combobox (callable externally) ---
    function setComboEnabled($input, enabled) {
        const $wrapper = $input.closest('.combo-container');

        $input.prop('disabled', !enabled);
        $input.attr('aria-disabled', (!enabled).toString());
        $wrapper.toggleClass('disabled', !enabled);
        $wrapper.find('.combo-toggle, .combo-clear').attr('aria-disabled', (!enabled).toString());

        if (!enabled) {
            $wrapper.find('.combo-suggestions').hide();
            $input.blur();
        }
    }
    window.setComboEnabled = setComboEnabled;

    // --- Main combobox init ---
    function initComboBox($input) {
        const url = $input.data('url');
        const valueField = $input.data('value-field');
        const textField = $input.data('text-field');
        const displayFields = ($input.data('display-fields') || textField).toString().split(',').map(f => f.trim());
        const selectedId = $input.data('selected-id');
        const placeholder = $input.data('placeholder') || '';
        const allowAdd = $input.data('allow-add') === true || $input.data('allow-add') === "true";

        const $wrapper = $input.closest('.combo-container');
        const $clearBtn = $wrapper.find('.combo-clear');
        const $dropdown = $wrapper.find('.combo-suggestions');
        const $toggleBtn = $wrapper.find('.combo-toggle');

        $input.attr({ 'autocomplete': 'off', 'placeholder': placeholder });

        let dataItems = [];

        if (url && /{\w+}/.test(url) && !hasAllRouteParams(url, $input)) {
            setComboEnabled($input, false);
        } else {
            setComboEnabled($input, true);
        }

        function formatValue(field, value) {
            if (field.toLowerCase().includes("amount") || field.toLowerCase().includes("balance")) {
                let num = parseFloat(value);
                if (!isNaN(num)) {
                    return num.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
                }
            }
            return value;
        }

        function renderItem(item) {
            const columns = displayFields.map(field =>
                `<div class="combo-col">${item[field] != null ? formatValue(field, item[field]) : ''}</div>`
            );
            return `<li data-id="${item[valueField]}" class="combo-item-row">${columns.join('')}</li>`;
        }

        function fetchAndRender(query) {
            if (!url) return;
            const fullUrl = buildUrl(url, $input);
            if (!fullUrl) {
                $dropdown.hide();
                return;
            }

            $.getJSON(fullUrl, { search: query }, function (data) {
                dataItems = data || [];

                const q = (query || '').toString().toLowerCase();
                const filtered = dataItems.filter(item =>
                    displayFields.some(field =>
                        (item[field] || '').toString().toLowerCase().includes(q)
                    )
                );

                let list = filtered.map(renderItem);

                if (filtered.length === 0 && allowAdd && query) {
                    list.push(`<li data-id="new" class="combo-item-row"><div class="combo-col">➕ Add "${query}"</div></li>`);
                }

                if (list.length > 0) {
                    $dropdown.html(list.join('')).show();
                } else {
                    $dropdown.hide();
                }
            });
        }

        $input.on('input focus', function () {
            if ($input.prop('disabled') || $wrapper.hasClass('disabled')) return;
            const query = $input.val().toLowerCase().trim();
            $clearBtn.toggle(!!$input.val());
            fetchAndRender(query);
        });

        $dropdown.on('click', 'li.combo-item-row', function () {
            if ($wrapper.hasClass('disabled')) return;
            const id = $(this).data('id');

            if (id === 'new') {
                alert(`Trigger 'Add New' for: "${$input.val()}"`);
                $dropdown.hide();
                return;
            }

            const item = dataItems.find(x => String(x[valueField]) === String(id));
            if (item) {
                $input.val(item[textField] || '')
                    .data('selected-id', item[valueField])
                    .data('selected-item', item)
                    .trigger('change');
                $clearBtn.show();
            }
            $dropdown.hide();
        });

        $clearBtn.on('click', function (e) {
            e.preventDefault();
            if ($wrapper.hasClass('disabled')) return;

            $input.val('')
                .removeAttr('data-selected-id')
                .removeData('selected-item')
                .trigger('change');

            $clearBtn.hide();
            fetchAndRender('');
            $dropdown.show();
            $input.focus();
        });

        $toggleBtn.on('click', function (e) {
            e.preventDefault();
            if ($wrapper.hasClass('disabled') || $input.prop('disabled')) return;
            if ($dropdown.is(':visible')) {
                $dropdown.hide();
            } else {
                fetchAndRender('');
                $dropdown.show();
                $input.focus();
            }
        });

        $(document).on('click', function (e) {
            if (!$(e.target).closest($wrapper).length) {
                $dropdown.hide();
            }
        });

        if (selectedId) {
            const fullUrl = buildUrl(url, $input);
            if (fullUrl) {
                $.getJSON(fullUrl, { id: selectedId }, function (data) {
                    const selectedItem = Array.isArray(data)
                        ? data.find(x => String(x[valueField]) === String(selectedId))
                        : data;

                    if (selectedItem) {
                        $input.val(selectedItem[textField] || '')
                            .data('selected-id', selectedItem[valueField])
                            .data('selected-item', selectedItem);
                        $clearBtn.show();
                    }
                });
            }
        }
    }

    // Global initializer
    window.initComboBoxes = function () {
        $('.combo-input').each(function () {
            initComboBox($(this));
        });
    };
})(jQuery);
