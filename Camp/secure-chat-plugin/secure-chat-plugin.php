<?php
/**
 * Plugin Name:       Secure Chat Plugin
 * Plugin URI:        https://example.com/
 * Description:       Provides a secure chat interface that proxies requests to a backend server.
 * Version:           1.0.0
 * Author:            [Your Name]
 * Author URI:        https://example.com/
 * License:           GPL v2 or later
 * License URI:       https://www.gnu.org/licenses/gpl-2.0.html
 * Text Domain:       secure-chat-plugin
 */

// If this file is called directly, abort.
if ( ! defined( 'ABSPATH' ) ) {
    die;
}

/**
 * Get the real IP address of the user, considering proxies and load balancers.
 *
 * @return string The user's IP address.
 */
function scp_get_user_ip_address() {
    $ip = '';
    if ( ! empty( $_SERVER['HTTP_CLIENT_IP'] ) ) {
        // IP from shared internet
        $ip = $_SERVER['HTTP_CLIENT_IP'];
    } elseif ( ! empty( $_SERVER['HTTP_X_FORWARDED_FOR'] ) ) {
        // IP passed from proxy
        $ip = $_SERVER['HTTP_X_FORWARDED_FOR'];
    } else {
        // Regular IP
        $ip = $_SERVER['REMOTE_ADDR'];
    }
    // Sanitize the IP address
    return esc_sql( wp_unslash( $ip ) );
}

/**
 * ===================================================================
 * Admin Settings Page (FR-1)
 * ===================================================================
 */

// Add the settings page to the admin menu
function scp_add_admin_menu() {
    add_options_page(
        'Secure Chat Settings',
        'Secure Chat',
        'manage_options',
        'secure_chat_plugin',
        'scp_settings_page_html'
    );
}
add_action('admin_menu', 'scp_add_admin_menu');

// Register the settings
function scp_settings_init() {
    register_setting('scp_plugin_page', 'scp_settings');

    add_settings_section(
        'scp_plugin_page_section',
        __('Backend API Configuration', 'secure-chat-plugin'),
        null,
        'scp_plugin_page'
    );

    add_settings_field(
        'scp_backend_url',
        __('Backend Server URL', 'secure-chat-plugin'),
        'scp_backend_url_callback',
        'scp_plugin_page',
        'scp_plugin_page_section'
    );

    add_settings_field(
        'scp_api_key',
        __('API Key', 'secure-chat-plugin'),
        'scp_api_key_callback',
        'scp_plugin_page',
        'scp_plugin_page_section'
    );
}
add_action('admin_init', 'scp_settings_init');

// Field callback functions
function scp_backend_url_callback() {
    $options = get_option('scp_settings');
    ?>
    <input type="url" name="scp_settings[scp_backend_url]" value="<?php echo isset($options['scp_backend_url']) ? esc_attr($options['scp_backend_url']) : ''; ?>" class="regular-text" placeholder="https://your-backend-server.com/api/chat">
    <p class="description"><?php _e('Enter the full URL of your backend API endpoint.', 'secure-chat-plugin'); ?></p>
    <?php
}

function scp_api_key_callback() {
    $options = get_option('scp_settings');
    ?>
    <input type="password" name="scp_settings[scp_api_key]" value="<?php echo isset($options['scp_api_key']) ? esc_attr($options['scp_api_key']) : ''; ?>" class="regular-text">
    <p class="description"><?php _e('Your secret API key. This will not be exposed to the public.', 'secure-chat-plugin'); ?></p>
    <?php
}

// Render the settings page form
function scp_settings_page_html() {
    if (!current_user_can('manage_options')) {
        return;
    }
    ?>
    <div class="wrap">
        <h1><?php echo esc_html(get_admin_page_title()); ?></h1>
        <form action="options.php" method="post">
            <?php
            settings_fields('scp_plugin_page');
            do_settings_sections('scp_plugin_page');
            submit_button('Save Changes');
            ?>
        </form>
    </div>
    <?php
}

/**
 * ===================================================================
 * Shortcode & Asset Enqueueing (FR-2)
 * ===================================================================
 */

// Shortcode function to render the chat widget HTML
function scp_render_chat_widget_shortcode() {
    // Enqueue scripts and styles only when the shortcode is used
    scp_enqueue_assets();

    // The HTML structure for the chat widget. JS will populate and control it.
    ob_start();
    ?>
    <div id="scp-chat-container">
        <div id="scp-chat-window" class="scp-is-hidden">
            <div class="scp-chat-header">
                <span>Chat with us</span>
                <button id="scp-chat-close">×</button>
            </div>
            <div id="scp-chat-messages"></div>
            <div id="scp-typing-indicator" class="scp-is-hidden">
                <span></span><span></span><span></span>
            </div>
            <form id="scp-chat-form">
                <input type="text" id="scp-chat-input" placeholder="Type your message..." autocomplete="off">
                <button type="submit">Send</button>
            </form>
        </div>
        <button id="scp-chat-toggle">
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" width="24" height="24"><path d="M20 2H4c-1.1 0-2 .9-2 2v18l4-4h14c1.1 0 2-.9 2-2V4c0-1.1-.9-2-2-2z"/></svg>
        </button>
    </div>
    <?php
    return ob_get_clean();
}
add_shortcode('secure_chat_widget', 'scp_render_chat_widget_shortcode');

// Enqueue CSS and JS assets
function scp_enqueue_assets() {
    // Enqueue Style
    wp_enqueue_style(
        'scp-chat-widget-style',
        plugin_dir_url(__FILE__) . 'assets/css/chat-widget.css',
        [],
        '1.0.0'
    );

    // Enqueue Script
    wp_enqueue_script(
        'scp-chat-widget-script',
        plugin_dir_url(__FILE__) . 'assets/js/chat-widget.js',
        ['jquery'], // Depends on jQuery
        '1.0.0',
        true // Load in footer
    );

    // Pass data to JavaScript (NFR-1 Security)
    wp_localize_script('scp-chat-widget-script', 'scp_chat_params', [
        'ajax_url' => admin_url('admin-ajax.php'),
        'nonce'    => wp_create_nonce('scp_chat_nonce')
    ]);
}


/**
 * ===================================================================
 * Backend Communication Proxy (FR-3)
 * ===================================================================
 */

function scp_handle_chat_message() {
    // 1. Security Check: Verify nonce (NFR-1)
    check_ajax_referer('scp_chat_nonce', 'nonce');

    // NEW: Get the user's IP address using our helper function.
    $user_ip = scp_get_user_ip_address();

    // 2. Get data from the client
    $chat_history_json = isset($_POST['chat_history']) ? stripslashes($_POST['chat_history']) : '[]';
    $chat_history = json_decode($chat_history_json, true);

    // Sanitize the chat history content
    if (is_array($chat_history)) {
        foreach ($chat_history as $key => $message) {
            if (isset($message['content'])) {
                 $chat_history[$key]['content'] = sanitize_text_field($message['content']);
            }
        }
    } else {
        wp_send_json_error(['message' => 'Invalid chat history format.']);
    }

    // 3. Get plugin settings securely from the database
    $options = get_option('scp_settings');
    $backend_url = isset($options['scp_backend_url']) ? esc_url_raw($options['scp_backend_url']) : '';
    $api_key = isset($options['scp_api_key']) ? $options['scp_api_key'] : '';

    if (empty($backend_url) || empty($api_key)) {
        wp_send_json_error(['message' => 'Admin error: Backend URL or API Key is not configured.']);
    }

    // 4. Make the server-to-server request to the backend
    // --- Start of Replacement ---

    // 4.1. Explicitly prepare the data payload as a PHP array.
    $payload = [
        'chatHistory' => $chat_history,
        'userIpAddress' => $user_ip // NEW: Add the IP address to the payload
    ];

    // 4.2. Prepare the arguments for wp_remote_post.
    $args = [
        'method'    => 'POST',
        'timeout'   => 45,
        'headers'   => [
            'Content-Type'  => 'application/json; charset=utf-8', // Be more specific with charset
            'Authorization' => 'Bearer ' . $api_key,
            'Accept'        => 'application/json', // Tell the server we expect JSON back
        ],
        // 4.3. Encode the payload and assign it to 'body'.
        // This is the most critical part. We are ensuring the body is a pure JSON string.
        'body'      => json_encode($payload),
        'data_format' => 'body', // This tells WordPress not to re-process the body as form data.
    ];

    // 4.4. Make the server-to-server request.
    $response = wp_remote_post($backend_url, $args);

    // --- End of Replacement ---

    // 5. Handle the response from the backend
    if (is_wp_error($response)) {
        wp_send_json_error(['message' => 'Error connecting to the backend server.']);
    }

    $response_code = wp_remote_retrieve_response_code($response);
    $response_body = wp_remote_retrieve_body($response);
    $data = json_decode($response_body, true);

    if ($response_code !== 200 || !$data) {
        wp_send_json_error(['message' => 'Received an invalid response from the backend server.']);
    }

    // 6. Send the successful response back to the client
    wp_send_json_success($data);
}
// Hook for both logged-in and logged-out users (as per User Personas)
add_action('wp_ajax_nopriv_send_chat_message', 'scp_handle_chat_message');
add_action('wp_ajax_send_chat_message', 'scp_handle_chat_message');